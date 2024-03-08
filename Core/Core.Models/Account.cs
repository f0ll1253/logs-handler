namespace Core.Models;

public record Account(
    string Username,
    string Password,
    string Url = "",
    string Log = ""
) : IDisposable, IEqualityComparer<Account>
{
    public void Dispose() =>
        GC.SuppressFinalize(this);
    
    public override string ToString() =>
        $"{Username}:{Password}:{Url}";

    public string ToStringShort() =>
        $"{Username}:{Password}";

    public bool Equals(Account? x, Account? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        
        return x.Username == y.Username && x.Password == y.Password;
    }

    public int GetHashCode(Account obj)
    {
        return HashCode.Combine(obj.Username, obj.Password, obj.Url, obj.Log);
    }
}