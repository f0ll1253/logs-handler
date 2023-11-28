namespace Console.Models.Abstractions;

public interface IViewResult<T> : IView
{
    Task<T?> Build();
}