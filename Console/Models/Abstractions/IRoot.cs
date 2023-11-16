namespace Console.Models.Abstractions;

public interface IRoot
{
    public Stack<IView> Views { get; }

    Task Start(CancellationToken? token = null);
    
    Task Redirect<T>(T view)
        where T : IView;

    Task Redirect<T>()
        where T : IView;
}