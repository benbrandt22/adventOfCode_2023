namespace Core.Shared.Modules;

public interface IDayModule
{
    public int Day { get; }
    public void Execute();
}