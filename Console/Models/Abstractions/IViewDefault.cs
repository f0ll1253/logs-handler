namespace Console.Models.Abstractions;

public interface IViewDefault : IView
{
    IRoot Root { get; set; }

    Task Build();
}