using Orderstamp.Models;

namespace Orderstamp.Scheduler;

internal class SchedulerOutput
{
    private readonly bool _step;
    private readonly bool _summaryOnly;
    private readonly bool _useColor;

    public SchedulerOutput(bool step, bool summaryOnly, bool useColor)
    {
        _step = step;
        _summaryOnly = summaryOnly;
        _useColor = useColor;
    }

    public void ExecutionHeader(int execution)
    {
        if (!_summaryOnly)
            Console.WriteLine($"\nExecution #{execution}");
    }

    public void Operation(int step, string message, ConsoleColor color)
    {
        if (_summaryOnly)
            return;

        WriteLine($"[{step}] {message}", color);
    }

    public void State(Operation operation, State state)
    {
        if (_summaryOnly)
            return;

        if (!string.IsNullOrEmpty(operation.Value))
        {
            var timestamp = state.Values[operation.Value];
            Console.WriteLine(
                $"    Item {operation.Value}: RTS = {timestamp.RTS}, WTS = {timestamp.WTS}");
        }

        Console.WriteLine(
            $"    Transactions: {{ {string.Join(", ", state.Status.Select(status => $"T{status.Key}:{status.Value}"))} }}");
    }

    public void Pause()
    {
        if (!_step)
            return;

        var promptRow = Console.IsOutputRedirected ? -1 : Console.CursorTop;
        Console.Write("    Press Enter to continue...");
        Console.ReadLine();
        ClearPrompt(promptRow);
    }

    public void Restart(IEnumerable<int> transactionIds)
    {
        if (!_summaryOnly)
        {
            WriteLine(
                $"\nRestarting aborted transactions: {string.Join(", ", transactionIds.Select(id => $"T{id}"))}",
                ConsoleColor.Blue);
        }
    }

    public void Summary(State state)
    {
        Console.WriteLine("\nFinal summary");
        Console.WriteLine("-------------");
        Console.WriteLine($"Final history: {string.Join("; ", state.FinalOperations)}");

        if (state.AutoAborts.Count > 0)
        {
            Console.WriteLine("\nAutomatic aborts:");
            state.AutoAborts.ForEach(abort => Console.WriteLine($"  - {abort}"));
        }

        Console.WriteLine("\nItem timestamps:");
        foreach (var item in state.Values.OrderBy(item => item.Key))
            Console.WriteLine($"  - {item.Key}: RTS = {item.Value.RTS}, WTS = {item.Value.WTS}");

        Console.WriteLine("\nTransactions:");
        foreach (var transaction in state.Status.OrderBy(transaction => transaction.Key))
        {
            Console.WriteLine(
                $"  - T{transaction.Key}: {transaction.Value} " +
                $"(TS = {state.TransactionTS[transaction.Key]})");
        }
    }

    private void WriteLine(string message, ConsoleColor color)
    {
        if (_useColor)
            Console.ForegroundColor = color;

        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void ClearPrompt(int promptRow)
    {
        if (promptRow < 0 || Console.IsOutputRedirected)
            return;

        var width = Math.Max(1, Console.BufferWidth - 1);
        Console.SetCursorPosition(0, promptRow);
        Console.Write(new string(' ', width));
        Console.SetCursorPosition(0, promptRow);
    }
}
