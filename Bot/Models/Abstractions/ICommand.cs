namespace Bot.Models.Abstractions;

public interface ICommand<in TUpdate> where TUpdate : Update {
    System.Threading.Tasks.Task ExecuteAsync(TUpdate update, User user);
}