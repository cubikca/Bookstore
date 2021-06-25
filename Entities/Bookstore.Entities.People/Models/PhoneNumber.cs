using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class PhoneNumber : IEntity
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public bool Deleted { get; set; }
        public string AreaCode { get; set; }
        public string Phone { get; set; }
        public string Extension { get; set; }

        public override string ToString()
        {
            return $"({AreaCode}) {Phone}" + (!string.IsNullOrEmpty(Extension) ? $"x{Extension}" : "");
        }
   }
}
