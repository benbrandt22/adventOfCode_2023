namespace Core.Shared;

/// <summary>
///     Provides shared methods to simplify running async code in a synchronous manner,
///     using a generally regarded "safer" process than using .Result or Task.Run().
///     The implementation itself came from private classes within the framework.
///     More information available here: https://cpratt.co/async-tips-tricks/
/// </summary>
/// <example>
///     <code>
/// public object GetData()
/// {
///     return AsyncHelper.RunSync(() => GetDataAsync());
/// }
/// </code>
/// </example>
public static class AsyncHelper
{
    private static readonly TaskFactory TaskFactory = new(CancellationToken.None,
        TaskCreationOptions.None,
        TaskContinuationOptions.None,
        TaskScheduler.Default);

    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        => TaskFactory
            .StartNew(func)
            .Unwrap()
            .GetAwaiter()
            .GetResult();

    public static void RunSync(Func<Task> func)
        => TaskFactory
            .StartNew(func)
            .Unwrap()
            .GetAwaiter()
            .GetResult();
}