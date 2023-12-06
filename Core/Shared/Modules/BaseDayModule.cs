namespace Core.Shared.Modules;

public abstract class BaseDayModule : IDayModule
{
    private readonly ITestOutputHelper _outputHelper;
    private InputDataProvider _inputDataProvider;

    public BaseDayModule(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _inputDataProvider = new InputDataProvider();
        
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
    /// When set to true, calls to Debug() will be written to the output
    /// </summary>
    public static bool DebugEnabled { get; set; }
    
    internal void WriteLine(string line = "") => _outputHelper.WriteLine(line);

    internal void Debug(string line = "")
    {
        if (DebugEnabled)
        {
            WriteLine(line);
        }
    }

    internal void WriteHorizontalRule(int length = 80) => WriteLine(new string('-', length));
}