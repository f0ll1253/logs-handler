using System.Reflection;
using Core.View.Attributes;
using Core.View.Models;
using Core.View.Models.Abstractions;

namespace Core.View;

public abstract class ArgsView : BaseView
{
    private readonly Dictionary<int, string> _args;
    
    protected ArgsView(IRoot root) : base(root)
    {
        _args = GetType()
            .GetTypeInfo()
            .DeclaredMethods
            .Where(x => x.GetCustomAttribute<CommandAttribute>() != null)
            .Select((x, i) =>
            {
                var attribute = x.GetCustomAttribute<CommandAttribute>()!;

                return (attribute.Index == 0 ? i : attribute.Index, x.Name);
            })
            .OrderBy(x => x.Item1)
            .ToDictionary(x => x.Item1, x => x.Name);
        
        _args.Add(99, "Exit");
    }
    
    public override Task Build()
    {
        foreach (var (i, cmd) in _args) Console.WriteLine($"{i}. {cmd.Replace('_', ' ')}");

        Console.Write("Choice: ");
        
        _args.TryGetValue(
            int.TryParse(Console.ReadLine() ?? "", out var index) 
                ? index 
                : -1, 
            out var value);
        
        return ExecuteMethod(value ?? "");
    }

    private Task ExecuteMethod(string name)
    {
        if (name.ToLower() == "exit")
        {
            Root.Views.Pop();
            return Task.CompletedTask;
        }
        
        var method = GetType()
            .GetTypeInfo()
            .DeclaredMethods
            .FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.GetCustomAttribute<CommandAttribute>() != null);

        if (method is null) return Task.CompletedTask; // todo add logging

        return method.ReturnType == typeof(Task)
            ? (Task) method.Invoke(this, null)!
            : Task.FromResult(method.Invoke(this, null));
    }
}