namespace Console.Models.Abstractions;

public interface IView : IDisposable
{
    void Initialize();
    void Activate();
    void Deactivate();
}