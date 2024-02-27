namespace Server.Models.Abstract;

public interface IRequestsMultipleParser<T> : IMultipleParser<T>
    where T : PrivateRequest
{
}