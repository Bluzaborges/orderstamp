using Orderstamp.Cli;

try
{
    return InteractiveShell.Run();
}
catch (Exception exception)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"Error: {exception.Message}");
    Console.ResetColor();
    return 1;
}
