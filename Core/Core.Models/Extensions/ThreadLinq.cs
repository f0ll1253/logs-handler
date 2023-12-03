namespace Core.Models.Extensions;

public static class ThreadLinqExtensions
{
    public static IEnumerable<TResult> SelectThread<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult?> func)
    {
        var list = new List<TResult>();
        var manual = new ManualResetEvent(false);
        ThreadPool.GetMaxThreads(out var workers, out _);
        var groups = enumerable
            .Select((x, i) => new {Index = i, Value = x})
            .GroupBy(x => x.Index / (workers - 10));

        foreach (var group in groups)
        {
            var complited = group.Count();

            foreach (var x in group)
            {
                ThreadPool.QueueUserWorkItem(state =>
                    {
                        if (func.Invoke(x.Value) is { } result)
                            list.Add(result);

                        if (Interlocked.Decrement(ref complited) == 0) state.Set();
                    },
                    manual,
                    true);
            }

            manual.WaitOne();
            manual.Reset();
        }

        return list;
    }
}