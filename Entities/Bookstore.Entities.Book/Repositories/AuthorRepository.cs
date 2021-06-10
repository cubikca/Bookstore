using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.Book.Repositories
{
    public class AuthorRepository : RepositoryBase<Author, Models.Author>, IAuthorRepository
    {
        public AuthorRepository(IDbContextFactory<BookContext> dbFactory, IMapper mapper, ILogger<AuthorRepository> logger) : base(dbFactory, mapper, logger)
        {
        }
    }
}