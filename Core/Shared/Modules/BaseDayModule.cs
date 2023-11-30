namespace Core.Shared.Modules;

public abstract class BaseDayModule : IDayModule
{
    public abstract int Day { get; }
    public abstract void Execute();

    /// <summary>
    /// When set to true, calls to Debug() will be written to the console output
    /// </summary>
    public static bool DebugEnabled { get; set; }
    
    internal static void WriteLine(string line = "") => Console.Out.WriteLine(line);

    internal static void Debug(string line = "")
    {
        if (DebugEnabled)
        {
            Console.Out.WriteLine(line);
        }
    }

    internal void WriteHorizontalRule(int length = 80) => WriteLine(new string('-',80));
}