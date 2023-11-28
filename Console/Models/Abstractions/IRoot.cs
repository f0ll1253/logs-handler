namespace Console.Models.Abstractions;

public interface IRoot
{
    Task Start(CancellationToken? token = null);

    Task<T> Show<T>(IViewResult<T> view);
    
    void Pop();
    
    void PushRedirect<T>(T view)
        where T : IViewDefault;

    void PushRedirect<T>()
        where T : IViewDefault;
}