namespace Core.Shared;

public static class TextFileLoader
{
    public static string LoadText(string fileName) => File.ReadAllText(GetFullPath(fileName));

    public static IEnumerable<string> LoadLines(string fileName) => File.ReadLines(GetFullPath(fileName));

    private static string GetFullPath(string fileName) => Path.Combine(AppContext.BaseDirectory, fileName);
}