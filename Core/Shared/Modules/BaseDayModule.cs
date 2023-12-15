namespace Core.Shared.Modules;

public abstract class BaseDayModule : XunitContextBase, IDayModule
{
    private InputDataProvider _inputDataProvider;

    public BaseDayModule(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _inputDataProvider = new InputDataProvider();

        // enable debug output if the test method has the ShowDebug attribute
        var testMethodInfo = Context.MethodInfo;
        var showDebugAttribute = (ShowDebugAttribute?)testMethodInfo.GetCustomAttributes(typeof(ShowDebugAttribute), false).FirstOrDefault();
        DebugEnabled = showDebugAttribute != null;
        
        // the test class gets instantiated once per test, so we can output the header here to make it display at the start of each unit test
        OutputHeader();
    }

    private void OutputHeader()
    {
        WriteHorizontalRule();
        WriteLine($"Advent of Code 2023 - Day {Day}: {Title}");
        WriteHorizontalRule();
    }

    public int Year => 2023;
    public abstract int Day { get; }
    public abstract string Title { get; }
    
    protected string GetData(InputType inputType) => _inputDataProvider.GetInputData(Year, Day, inputType);
    protected string GetData(string inputType) => _inputDataProvider.GetInputData(Year, Day, inputType);

    /// <summary>
    /// When set to true, calls to Debug() will be written to the output.
    /// This can also be enabled by adding the ShowDebug attribute to the test method.
    /// </summary>
    public static bool DebugEnabled { get; set; }
    
    internal void WriteLine(string line = "") => Console.WriteLine(line);

    internal void Debug(string line = "")
    {
        if (DebugEnabled)
        {
            System.Diagnostics.Debug.WriteLine(line);
        }
    }

    internal void WriteHorizontalRule(int length = 80) => WriteLine(new string('-', length));
}