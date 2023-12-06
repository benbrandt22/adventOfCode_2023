namespace Core.Shared.Modules;

public class InputDataProvider
{
    public string GetInputData(int year, int day, InputType inputType) =>
        GetInputData(year, day, inputType.ToString().ToLower());

    public string GetInputData(int year, int day, string inputType)
    {
        // try getting from the file system first
        var fileName = $"Day{day:00}/{inputType}.txt";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        if(!File.Exists(filePath))
        {
            throw new NotImplementedException("TODO: implement getting input data from the web");
        }
        return File.ReadAllText(filePath);
    }
}

public enum InputType
{
    Sample,
    Input
}