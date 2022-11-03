namespace Bookstore.Domain.People.Author;

using EventFramework.EventSourcing;

public class Author : AggregateRoot<Author>
{
    public Author(AggregateId<Author> id) : base(id)
    {
    }

    protected override void When(object? @event)
    {
    }

    protected override void EnsureValidState()
    {
    }
}