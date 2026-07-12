namespace FCG.UsersAPI.Domain.Comum;
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem) { }
}
