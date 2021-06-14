using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Bookstore.Domains.People.Models;

namespace Bookstore.Entities.People.Models
{
    public class Country : IEntity
    {
        public string Abbreviation { get; set; }
        public string Name { get; set; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Abbreviation), Abbreviation);
            info.AddValue(nameof(Name), Name);
        }

        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
