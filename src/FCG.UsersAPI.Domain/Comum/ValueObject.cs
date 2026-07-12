namespace FCG.UsersAPI.Domain.Comum;
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> ObterAtributosDeIgualdade();
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return ObterAtributosDeIgualdade()
            .SequenceEqual(((ValueObject)obj).ObterAtributosDeIgualdade());
    }
    public override int GetHashCode() =>
        ObterAtributosDeIgualdade()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate(0, (a, b) => a ^ b);
    public static bool operator ==(ValueObject? a, ValueObject? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}
