namespace Bookstore.Domain.People.Author;

using EventFramework.EventSourcing;

public class FamilyName : Value<FamilyName>
{
    private string Value { get; set; }

    public FamilyName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        Value = name;
    }

    public static FamilyName FromString(string? name) => new(name);

    public static implicit operator string?(FamilyName? name) => name?.Value;
}