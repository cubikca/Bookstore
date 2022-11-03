namespace Bookstore.Domain.People.Author;

using EventFramework.EventSourcing;

public class Birthdate : Value<Birthdate>
{
    private DateTime Value { get; set; }

    public Birthdate(DateTime? birthdate)
    {
        if (birthdate == null)
            throw new ArgumentNullException(nameof(birthdate));
        Value = birthdate.Value.Date;
    }

    public static Birthdate FromDateTime(DateTime? birthdate) => new(birthdate);

    public static implicit operator DateTime?(Birthdate? birthdate) => birthdate?.Value;
}