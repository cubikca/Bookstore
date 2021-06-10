using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Bookstore.Entities.People.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bookstore.Entities.People
{
    public class PeopleContext : DbContext
    {
        public PeopleContext(DbContextOptions<PeopleContext> dbopt) : base(dbopt)
        {
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<PersonGivenName> PersonGivenNames { get; set; }
        public DbSet<PersonKnownAsName> PersonKnownAsNames { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<LocationContact> LocationContacts { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<EmailAddress> EmailAddresses { get; set; }
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public DbSet<OnlinePresence> OnlinePresence { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocationContact>().HasKey(lc => new {lc.LocationId, lc.ContactId});
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IEntity).IsAssignableFrom(entityType.ClrType) && entityType.ClrType.BaseType == null)
                    entityType.AddDeletedQueryFilter();
            }
            modelBuilder.Entity<Subject>()
                .Ignore(s => s.MailingAddress)
                .Ignore(s => s.StreetAddress);
        }
    }

     public static class DeletedQueryExtension
     {
         public static void AddDeletedQueryFilter(this IMutableEntityType entityData)
         {
             var methodToCall = typeof(DeletedQueryExtension)
                 .GetMethod(nameof(GetDeletedFilter), BindingFlags.NonPublic | BindingFlags.Static)?
                 .MakeGenericMethod(entityData.ClrType);
             var filter = methodToCall?.Invoke(null, new object[] { });
             if (filter != null)
                 entityData.SetQueryFilter((LambdaExpression) filter);
             entityData.AddIndex(entityData.FindProperty(nameof(IEntity.Deleted)));
         }
 
         private static LambdaExpression GetDeletedFilter<TEntity>()
             where TEntity : class, IEntity
         {
             Expression<Func<TEntity, bool>> filter = x => !x.Deleted;
             return filter;
         }
    }
}


