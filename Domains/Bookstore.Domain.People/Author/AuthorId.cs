namespace Bookstore.Domain.People.Author;

using EventFramework.EventSourcing;

public class AuthorId : AggregateId<Author>
{
    private AuthorId(Ulid value) : base(value.ToString())
    {
    }

    public static AuthorId FromUlid(Ulid? ulid)
    {
        if (ulid == null) throw new ArgumentNullException(nameof(ulid));
        return new(ulid.Value);
    }

    public static implicit operator string?(AuthorId? id) => id?.Value;

    public static implicit operator Ulid?(AuthorId? id) => id != null ? Ulid.Parse(id.Value) : null;
}