using System.Reflection;
using Microsoft.Extensions.Configuration;

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
            if (inputType.Equals(InputType.Input.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                AsyncHelper.RunSync(() => DownloadAdventOfCodeInputFileAsync(year, day, filePath));
            }
        }
        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// Downloads the specified AOC input file to the specified path.
    /// </summary>
    /// <remarks>
    /// Adapted from https://www.nuget.org/packages/AdventOfCodeSupport
    /// </remarks>
    private async Task DownloadAdventOfCodeInputFileAsync(int year, int day, string filePath)
    {
        var configBuilder = new ConfigurationBuilder();
        // the type specified here is just so the secrets library can find the UserSecretId in the csproj file
        configBuilder.AddUserSecrets<InputDataProvider>();
        var config = configBuilder.Build();
        var cookie = config["session"];
        if (string.IsNullOrWhiteSpace(cookie))
        {
            throw new InvalidOperationException(
                "Unable to get input from AdventOfCode.com. No session cookie found. See ReadMe for details.");
        }
        
        var handler = new HttpClientHandler { UseCookies = false };
        var adventHttpClient = new HttpClient(handler) { BaseAddress = new Uri("https://adventofcode.com/") };
        adventHttpClient.DefaultRequestHeaders.Add("cookie", $"session={cookie.Trim()}");
        
        var result = await adventHttpClient.GetAsync($"{year}/day/{day}/input");
        if (!result.IsSuccessStatusCode)
            throw new Exception($"Input download {year} Day {day} failed. {result.ReasonPhrase}");

        var text = await result.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(filePath, text);
        await Task.Delay(1000); // Rate limit input downloads.
    }
}

public enum InputType
{
    Sample,
    Input
}