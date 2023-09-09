using Core.View.Models.Abstractions;

namespace Core.View.Models;

public abstract class BaseView : IView
{
    protected BaseView(IRoot root)
    {
        Root = root;
    }
    
    public IRoot Root { get; set; }
    
    public virtual void Initialize() { }

    public virtual void SetState(Action action)
    {
        action.Invoke();
        
        Build();
    }

    public virtual Task Build() => Task.CompletedTask;

    public virtual void Dispose() { }
}