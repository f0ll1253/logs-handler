using System.Reflection;
using Core.View.Attributes;
using Core.View.Models;
using Core.View.Models.Abstractions;

namespace Core.View;

public abstract class ArgsView : BaseView
{
    private readonly Dictionary<int, string> _args = new ();
    
    protected ArgsView(IRoot root) : base(root)
    {
        var args = GetType()
            .GetTypeInfo()
            .DeclaredMethods
            .Where(x => x.GetCustomAttribute<CommandAttribute>() != null)
            .Select(x => x.Name)
            .Select(x => x.Replace('_', ' '))
            .ToArray();
        
        for (int i = 0; i < args.Length; i++) _args.Add(i, args[i]);
        _args.Add(99, "Exit");
    }
    
    public override Task Build()
    {
        foreach (var (i, cmd) in _args) Console.WriteLine($"{i}. {cmd}");

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