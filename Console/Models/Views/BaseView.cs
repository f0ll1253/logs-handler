using Console.Models.Abstractions;

namespace Console.Models.Views;

public abstract class BaseView : IView
{
    protected BaseView(IRoot root)
    {
        Root = root;
    }
    
    public IRoot Root { get; set; }
    
    public virtual void Initialize() { }

    public virtual void Activate() { }
    
    public virtual void Deactivate() { }

    public virtual Task Build() => Task.CompletedTask;

    public virtual void Dispose() { }
}