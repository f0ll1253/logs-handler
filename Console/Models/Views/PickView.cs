using Console.Models.Abstractions;

namespace Console.Models.Views;

public class PickView<T> : IViewResult<T>
{
    private readonly Dictionary<int, (string, T)> _values;

    public PickView(Dictionary<string, T> values)
    {
        _values = values.Select((pair, i) => (i, pair)).ToDictionary(x => x.i, x => (x.pair.Key, x.pair.Value));
    }

    public Task<T?> Build()
    {
        foreach (var x in _values) System.Console.WriteLine($"{x.Key}. {x.Value.Item1}");

        if (!int.TryParse(System.Console.ReadKey(false).KeyChar.ToString(), out var i)) return (Task<T?>) Task.CompletedTask;
        
        if (!_values.TryGetValue(i, out var pair)) return (Task<T?>) Task.CompletedTask;
        
        return Task.FromResult<T?>(pair.Item2);
    }

    public void Dispose() { }
    public void Initialize() { }
    public void Activate() { }
    public void Deactivate() { }
}