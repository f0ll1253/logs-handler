namespace Bot.Models.Abstractions;

public interface IFilter<in T> {
    public int Order { get; }
    Task<bool> CanExecuteAsync(T obj);
}