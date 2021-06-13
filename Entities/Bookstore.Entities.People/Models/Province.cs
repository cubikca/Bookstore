using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Province : IEntity
    {
        public string Abbreviation { get; set; }
        // when we create a province, we create without country and add afterward, so this needs to be nullable
        public Guid? CountryId { get; set; }
        public string Name { get; set; }

        public virtual Country Country { get; set; }

        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
