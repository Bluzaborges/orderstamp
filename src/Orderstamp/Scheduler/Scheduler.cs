using Orderstamp.Enums;
using Orderstamp.Helpers;
using Orderstamp.Models;

namespace Orderstamp.Scheduler;

public static class Scheduler
{
    public static void Run(
        string history,
        bool stepByStep = false,
        bool summaryOnly = false,
        bool useColor = true)
    {
        var operations = HistoryHelper.ParseHistory(history);
        var state = State.Create();
        var output = new SchedulerOutput(stepByStep, summaryOnly, useColor);

        var operationsByTransaction = operations
            .GroupBy(operation => operation.TID)
            .ToDictionary(group => group.Key, group => group.ToList());

        var step = 0;
        var execution = 0;

        while (true)
        {
            execution++;
            output.ExecutionHeader(execution);

            foreach (var operation in operations)
            {
                step++;

                if (state.Status.GetValueOrDefault(operation.TID) == TxStatus.Aborted &&
                    operation.Type != 'b')
                {
                    output.Operation(
                        step,
                        $"{operation} ignored (T{operation.TID} aborted - awaiting restart)",
                        ConsoleColor.Yellow);
                    output.Pause();
                    continue;
                }

                var (message, color) = Execute(operation, state);
                output.Operation(step, message, color);
                output.State(operation, state);
                output.Pause();
            }

            var aborted = state.TransactionsToRetry.OrderBy(id => id).ToList();

            if (aborted.Count == 0)
            {
                output.Summary(state);
                return;
            }

            output.Restart(aborted);

            operations = aborted
                .SelectMany(transactionId =>
                {
                    var transactionOperations = operationsByTransaction[transactionId].ToList();

                    if (transactionOperations.All(operation => operation.Type != 'b'))
                        transactionOperations.Insert(0, Operation.Create('b', transactionId));

                    return transactionOperations;
                })
                .ToList();
        }
    }

    private static (string Message, ConsoleColor Color) Execute(Operation operation, State state)
    {
        switch (operation.Type)
        {
            case 'b':
                operation.Begin(state, operation.TID);
                return ($"b{operation.TID} OK; TS(T{operation.TID}) = {state.TransactionTS[operation.TID]}",
                    ConsoleColor.Blue);

            case 'r':
            {
                var message = operation.Read(state, operation);
                return (message, MessageColor(message));
            }

            case 'w':
            {
                var message = operation.Write(state, operation);
                return (message, MessageColor(message));
            }

            case 'c':
                operation.Commit(state, operation.TID);
                return ($"c{operation.TID} OK", ConsoleColor.Blue);

            case 'a':
                operation.Abort(state, operation.TID, "explicit abort");
                return ($"a{operation.TID} OK", ConsoleColor.Red);

            default:
                return ($"Unknown operation: {operation}", ConsoleColor.Red);
        }
    }

    private static ConsoleColor MessageColor(string message) =>
        message.Contains("VIOLATION", StringComparison.Ordinal)
            ? ConsoleColor.Red
            : ConsoleColor.Green;
}
