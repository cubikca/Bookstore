using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Country : ISerializable
    {
        [Key]
        public string Abbreviation { get; set; }
        public string Name { get; set; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Abbreviation), Abbreviation);
            info.AddValue(nameof(Name), Name);
        }
    }
}
