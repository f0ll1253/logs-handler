namespace Console.Models.Abstractions;

public interface IRoot
{
    Task Start(CancellationToken? token = null);

    void Pop();
    
    void PushRedirect<T>(T view)
        where T : IView;

    void PushRedirect<T>()
        where T : IView;
}