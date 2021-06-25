using System;

namespace Bookstore.Domains.Book
{
    public interface IResult
    {
        public int Status { get; set; }
        public bool Success { get; set; }
        
        public string Message { get; set; }
        public string Warning { get; set; }
        public string Error { get; set; }
    }
}