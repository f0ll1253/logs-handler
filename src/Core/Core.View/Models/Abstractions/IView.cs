namespace Core.View.Models.Abstractions;

public interface IView : IDisposable
{
    public IRoot Root { get; set; }

    void Initialize();
    void SetState(Action action);
    Task Build();
}