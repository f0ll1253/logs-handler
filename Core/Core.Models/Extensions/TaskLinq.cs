namespace Core.Models.Extensions;

public static class TaskLinqExtensions
{
    public static IEnumerable<TResult> SelectPerTask<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult?> func)
    {
        var list = new List<TResult>();
        var manual = new ManualResetEvent(false);

        var complited = enumerable.Count();

        foreach (var x in enumerable)
        {
            Task.Factory.StartNew(state =>
                {
                    if (func.Invoke(x) is { } result)
                        list.Add(result);

                    if (Interlocked.Decrement(ref complited) == 0) ((ManualResetEvent) state!).Set();
                },
                manual);
        }

        manual.WaitOne();
        manual.Reset();

        return list;
    }
}