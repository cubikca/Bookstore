namespace Bookstore.Domain.People.Author;

using EventFramework.EventSourcing;

public class GivenName : Value<GivenName>
{
    private string Value { get; set; }

    public GivenName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        Value = name;
    }

    public static GivenName FromString(string? name) => new(name);

    public static implicit operator string?(GivenName? name) => name?.Value;
}