using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    // A valid name has at minimum a family name and one given name
    public class Person : Subject, IEquatable<Person>, ISerializable
    {
        /// <summary>
        /// The honorific title for this person, e.g. Dr. Mr. Ms. Mrs.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// All of the names for this person except the family name
        /// </summary>
        public IList<string> GivenNames { get; set; }
        /// <summary>
        /// Additional conversational names for this person
        /// </summary>
        public IList<string> KnownAs { get; set; }
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

        public Person()
        {
            GivenNames = new List<string>();
            KnownAs = new List<string>();
        }

        public override string FullName
        {
            get
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrEmpty(Title))
                    builder.Append($"{Title} ");
                foreach (var name in GivenNames)
                    builder.Append($"{name} ");
                if (!string.IsNullOrEmpty(Initial))
                    builder.Append($"{Initial} ");
                builder.Append(FamilyName);
                if (!string.IsNullOrEmpty(Suffix))
                    builder.Append($" {Suffix}");
                return builder.ToString();
            }
        }

        // There are a lot of possibilities here but we can only have one
        // This one seems reasonable and any other combination can be added where it is needed
        public override string Name
        {
            get
            {
                var builder = new StringBuilder();
                if (GivenNames.Any())
                    builder.Append($"{GivenNames[0]} ");
                if (!string.IsNullOrEmpty(FamilyName))
                    builder.Append($"{FamilyName}");
                return builder.ToString();
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Title", Title);
            info.AddValue("FamilyName", FamilyName);
            info.AddValue("GivenNames", GivenNames);
            info.AddValue("KnownAs", KnownAs);
            info.AddValue("Initial", Initial);
            info.AddValue("Suffix", Suffix);
        }

        public bool Equals(Person other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var givenNamesEqual = GivenNames != null && other.GivenNames != null
                ? GivenNames.All(other.GivenNames.Contains) && other.GivenNames.All(GivenNames.Contains)
                : (GivenNames?.Count ?? 0) == (other.GivenNames?.Count ?? 0);
            var knownAsEqual = KnownAs != null && other.KnownAs != null
                ? KnownAs.All(other.KnownAs.Contains) && other.KnownAs.All(KnownAs.Contains)
                : (KnownAs?.Count ?? 0) == (other.KnownAs?.Count ?? 0);
            return Title == other.Title && givenNamesEqual && knownAsEqual && FamilyName == other.FamilyName && Initial == other.Initial && Suffix == other.Suffix;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Person other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, GivenNames, KnownAs, FamilyName, Initial, Suffix);
        }
    }
}
