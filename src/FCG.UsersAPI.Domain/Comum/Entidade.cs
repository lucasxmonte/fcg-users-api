namespace FCG.UsersAPI.Domain.Comum;
public abstract class Entidade
{
    public Guid Id { get; protected set; }
    protected Entidade() { Id = Guid.NewGuid(); }
    public override bool Equals(object? obj) =>
        obj is Entidade other && Id == other.Id && Id != Guid.Empty;
    public override int GetHashCode() => Id.GetHashCode();
}
