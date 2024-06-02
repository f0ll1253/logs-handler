namespace Bot.Models.Abstractions;

public interface IFilter<in T> {
    public int Order { get; set; }
    Task<bool> CanExecuteAsync(T obj);
}