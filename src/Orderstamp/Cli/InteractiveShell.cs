using Orderstamp.Scheduler;

namespace Orderstamp.Cli;

public static class InteractiveShell
{
    public static int Run()
    {
        Console.WriteLine("Orderstamp 1.0.0");
        Console.WriteLine("Type help to see the commands.\n");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (input is null)
            {
                Console.WriteLine();
                return 0;
            }

            input = input.Trim();
            if (input.Length == 0)
                continue;

            var (command, value) = SplitCommand(input);

            try
            {
                switch (command.ToLowerInvariant())
                {
                    case "help":
                    case "?":
                        PrintHelp();
                        break;

                    case "run":
                        RunHistory(value, step: false, summaryOnly: false);
                        break;

                    case "step":
                        RunHistory(value, step: true, summaryOnly: false);
                        break;

                    case "summary":
                        RunHistory(value, step: false, summaryOnly: true);
                        break;

                    case "file":
                        RunFile(value);
                        break;

                    case "clear":
                    case "cls":
                        Console.Clear();
                        break;

                    case "version":
                        Console.WriteLine("Orderstamp 1.0.0");
                        break;

                    case "exit":
                    case "quit":
                        return 0;

                    default:
                        if (LooksLikeHistory(input))
                            RunHistory(input, step: false, summaryOnly: false);
                        else
                            WriteError($"Unknown command: {command}");
                        break;
                }
            }
            catch (Exception exception)
            {
                WriteError(exception.Message);
            }
        }
    }

    private static void RunHistory(string history, bool step, bool summaryOnly)
    {
        if (string.IsNullOrWhiteSpace(history))
            throw new CliException("A history is required after the command.");

        Scheduler.Scheduler.Run(
            history,
            step,
            summaryOnly,
            !Console.IsOutputRedirected);
    }

    private static void RunFile(string path)
    {
        path = path.Trim().Trim('"');

        if (path.Length == 0)
            throw new CliException("A path is required after the file command.");

        if (!File.Exists(path))
            throw new CliException($"History file not found: {path}");

        RunHistory(File.ReadAllText(path), step: false, summaryOnly: false);
    }

    private static (string Command, string Value) SplitCommand(string input)
    {
        var separator = input.IndexOfAny([' ', '\t']);
        return separator < 0
            ? (input, string.Empty)
            : (input[..separator], input[(separator + 1)..].Trim());
    }

    private static bool LooksLikeHistory(string input)
    {
        var first = char.ToLowerInvariant(input[0]);
        return first is 'b' or 'r' or 'w' or 'c' or 'a';
    }

    private static void PrintHelp()
    {
        Console.WriteLine(
            """

            COMMANDS
              run <history>       Execute a history with detailed output
              step <history>      Execute and pause after every operation
              summary <history>   Execute and print only the final summary
              file <path>         Execute a history stored in a text file
              help                Show the available commands
              clear               Clear the terminal
              version             Show the program version
              exit                Close the program

            A history may also be entered directly without the run command.

            EXAMPLES
              run r1(X); w2(X); r2(Y); w1(X); c1; c2
              step r1(X); w2(X); c1; c2
              summary r1(X); w2(X); c1; c2
              file examples/history.txt
            """);
    }

    private static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Error: {message}");
        Console.ResetColor();
    }
}
