namespace Core.Shared.Modules;

public static class ModuleLoader
{
    /// <summary>
    /// Gets the module for the specified day. If day is not specified, gets the latest available (highest day)
    /// </summary>
    public static IDayModule GetModule(int? day = null)
    {
        var modules = GetAllModules().OrderBy(m => m.Day).ToList();
        if (day.HasValue)
        {
            return modules.FirstOrDefault(m => m.Day == day) ?? new DayNotFoundModule();
        }

        return modules.Last();
    }

    private static IEnumerable<IDayModule> GetAllModules() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(x => !x.IsAbstract && typeof(IDayModule).IsAssignableFrom(x))
            .Select(Activator.CreateInstance)!
            .Cast<IDayModule>();
}