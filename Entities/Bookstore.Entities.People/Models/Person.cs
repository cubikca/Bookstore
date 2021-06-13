using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    [Table("People")]
    public class Person : Subject
    {
        /// <summary>
        /// The honorific title for this person, e.g. Dr. Mr. Ms. Mrs.
        /// </summary>
        public Guid? StreetAddressId { get; set; }
        public Guid? MailingAddressId { get; set; }
        public Guid? EmailAddressId { get; set; }
        public Guid? PhoneNumberId { get; set; }
        public Guid? OnlinePresenceId { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// All of the names for this person except the family name
        /// </summary>
        public virtual IList<PersonGivenName> GivenNames { get; set; }
        /// <summary>
        /// Additional conversational names for this person
        /// </summary>
        public virtual IList<PersonKnownAsName> KnownAs { get; set; }
        /// <summary>
        /// Family name for this person
        /// </summary>
        public string FamilyName { get; set; }
        /// <summary>
        /// Initial to be displayed between all given names and the family name.
        /// If you want to abbreviate the middle name, do not add it as a given name
        /// and add the initial here.
        /// </summary>
        public string Initial { get; set; }
        /// <summary>
        /// Honorific suffix for this person, e.g. Jr., III
        /// </summary>
        public string Suffix { get; set; }

        public override string FullName
        {
            get
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrEmpty(Title))
                    builder.Append($"{Title} ");
                if (GivenNames != null)
                    foreach (var name in GivenNames)
                        builder.Append($"{name.GivenName} ");
                if (!string.IsNullOrEmpty(Initial))
                    builder.Append($"{Initial} ");
                builder.Append(FamilyName);
                if (!string.IsNullOrEmpty(Suffix))
                    builder.Append($" {Suffix}");
                return builder.ToString();
            }
        }

        public override string Name
        {
            get
            {
                var builder = new StringBuilder();
                if (GivenNames?.Any() == true)
                    builder.Append($"{GivenNames[0]} ");
                if (!string.IsNullOrEmpty(FamilyName))
                    builder.Append($"{FamilyName}");
                return builder.ToString();
            }
        }

        private Address _streetAddress;

        public override Address StreetAddress
        {
            get => _streetAddress;
            set => _streetAddress = value;
        }
        private Address _mailingAddress;

        public override Address MailingAddress
        {
            get => _mailingAddress; 
            set => _mailingAddress = value;
        }
        public virtual EmailAddress EmailAddress { get; set; }
        public virtual PhoneNumber PhoneNumber { get; set; }
        public virtual OnlinePresence OnlinePresence { get; set; }
    }
}
