using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class PhoneNumber
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string AreaCode { get; set; }
        public string Phone { get; set; }
        public string Extension { get; set; }

        public override string ToString()
        {
            return $"({AreaCode}) {Phone}" + (!string.IsNullOrEmpty(Extension) ? $"x{Extension}" : "");
        }
   }
}
