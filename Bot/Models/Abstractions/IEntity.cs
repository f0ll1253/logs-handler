namespace Bot.Models.Abstractions;

public interface IEntity<TKey> {
    TKey Id { get; set; }
}