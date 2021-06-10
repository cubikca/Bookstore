using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Bookstore.Entities.Book.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bookstore.Entities.Book
{
    public class BookContext : DbContext
    {
        public BookContext(DbContextOptions<BookContext> options) : base(options)
        {}
        
        public DbSet<Models.Book> Books { get; set; }
        public DbSet<Models.Author> Authors { get; set; }
        public DbSet<Models.Publisher> Publishers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             foreach (var entityType in modelBuilder.Model.GetEntityTypes())
             {
                 if (typeof(IEntity).IsAssignableFrom(entityType.ClrType) && entityType.ClrType.BaseType == null)
                     entityType.AddDeletedQueryFilter();
             }
             modelBuilder.Entity<Models.Book>()
                 .HasMany(b => b.Authors)
                 .WithMany(a => a.Books)
                 .UsingEntity<Dictionary<string, object>>("AuthorBooks",
                     x => x.HasOne<Author>().WithMany(),
                     x => x.HasOne<Models.Book>().WithMany());
             base.OnModelCreating(modelBuilder);
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
