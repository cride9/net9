using Microsoft.Extensions.AI;

namespace net9;

public static class Logger
{
    public static string CurrentAgent = "Orchestrator Agent";

    public static void Log(string name)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Function `{(name + "`").PadRight(20)} by {CurrentAgent}");
        Console.ResetColor();
    }
}
