using System.Reflection;
using Console.Models.Abstractions;
using Console.Models.Attributes;

namespace Console.Models.Views;

public abstract class ArgsView : BaseView
{
    private readonly Dictionary<int, MethodInfo> _args;
    
    protected ArgsView(IRoot root) : base(root)
    {
        _args = GetType()
            .GetTypeInfo()
            .DeclaredMethods
            .Where(x => x.GetCustomAttribute<CommandAttribute>() != null)
            .Select((x, i) =>
            {
                var attribute = x.GetCustomAttribute<CommandAttribute>()!;

                return (attribute.Index == 0 ? i : attribute.Index, x);
            })
            .OrderBy(x => x.Item1)
            .ToDictionary(x => x.Item1, x => x.Item2);

        System.Console.CursorVisible = false;
    }

    public override void Dispose()
    {
        System.Console.CursorVisible = true;
    }

    public override Task Build()
    {
        foreach (var (i, method) in _args) System.Console.WriteLine($"{i}. {method.Name.Replace('_', ' ')}");
        
        return Execute(System.Console.ReadKey(true));
    }

    private Task Execute(ConsoleKeyInfo key)
    {
        if (key.Key is ConsoleKey.Escape)
        {
            Root.Pop();
            Dispose();
            
            return Task.CompletedTask;
        }

        if (!int.TryParse(key.KeyChar.ToString(), out var index)) return Task.CompletedTask;

        if (!_args.TryGetValue(index, out var method)) return Task.CompletedTask;

        return method.ReturnType == typeof(Task)
            ? (Task) method.Invoke(this, null)!
            : Task.FromResult(method.Invoke(this, null));
    }
}