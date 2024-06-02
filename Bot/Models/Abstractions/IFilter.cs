namespace Bot.Models.Abstractions;

public interface IFilter<in T> {
    Task<bool> CanExecuteAsync(T obj);
}