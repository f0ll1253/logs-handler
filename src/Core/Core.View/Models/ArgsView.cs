using System.Reflection;
using Core.View.Models.Abstractions;
using Core.View.Models.Realizations;

namespace Core.View.Models;

public abstract class ArgsView : BaseView
{
    private readonly Dictionary<int, string> _args = new ();
    
    protected ArgsView(IRoot root) : base(root)
    {
        var args = this.GetType()
            .GetTypeInfo()
            .DeclaredMethods
            .Select(x => x.Name)
            .Select(x => x.Replace('_', ' '))
            .ToArray();
        
        for (int i = 0; i < args.Length; i++) _args.Add(i, args[i]);
        _args.Add(99, "Exit");
    }
    
    public override Task Build()
    {
        foreach (var (i, cmd) in _args) System.Console.WriteLine($"{i}. {cmd}");

        System.Console.Write("Choice: ");
        
        _args.TryGetValue(
            int.TryParse(System.Console.ReadLine() ?? "", out var index) 
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
        
        var method = this.GetType()
            .GetTypeInfo()
            .DeclaredMethods
            .FirstOrDefault(x => x.Name.ToLower() == name.ToLower());

        if (method is null) return Task.CompletedTask; // todo add logging

        return method.ReturnType == typeof(Task)
            ? (Task) method.Invoke(this, null)!
            : Task.FromResult(method.Invoke(this, null));
    }
}