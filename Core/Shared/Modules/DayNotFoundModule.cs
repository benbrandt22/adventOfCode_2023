namespace Core.Shared.Modules;

public class DayNotFoundModule : BaseDayModule
{
    public override int Day => -1;
    public override void Execute()
    {
        WriteLine("Module not found");
    }
}