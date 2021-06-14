using System;
using System.Runtime.Serialization;

namespace Bookstore.Domains.Book.Models
{
    public interface IDomainObject : ISerializable
    {
        Guid Id { get; set; }
        string CreatedBy { get; set; }
        DateTimeOffset Created { get; set; }
        string UpdatedBy { get; set; }
        DateTimeOffset Updated { get; set; }
    }
}