namespace Console.Models.Abstractions;

public interface IView : IDisposable
{
    public IRoot Root { get; set; }

    void Initialize();
    void Activate();
    void Deactivate();
    Task Build();
}