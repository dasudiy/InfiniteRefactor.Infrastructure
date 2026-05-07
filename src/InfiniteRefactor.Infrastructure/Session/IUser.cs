namespace InfiniteRefactor.Infrastructure.Session
{
    public interface IUser
    {
        string Username { get; }

        object Id { get; }

        string Realname { get; }

        string Email { get; }
    }
}
