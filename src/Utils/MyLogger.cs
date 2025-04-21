namespace BasicFaceitServer.Utils;

using System.Diagnostics;

public static class MyLogger
{
    public static void Info(string message)
    {
        Log(message, ConsoleColor.Green);
    }

    public static void Warn(string message)
    {
        Log(message, ConsoleColor.Yellow);
    }

    public static void Error(string message)
    {
        Log(message, ConsoleColor.Red);
    }

    public static void Debug(string message)
    {
        Log(message, ConsoleColor.Cyan);
    }

    private static void Log(string message, ConsoleColor color)
    {
        // Get the calling method (stack trace)
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(2); // 2 skips over the logging function itself
        var logFunc = stackTrace.GetFrame(1)?.GetMethod()?.Name;
        var methodName = frame?.GetMethod()?.Name;
        var className = frame?.GetMethod()?.DeclaringType?.Name;

        // Log the message with the class and method name
        Console.ForegroundColor = color;
        Console.WriteLine($"[{logFunc}] [{className}.{methodName}] {message}");
        Console.ResetColor();
    }
}