using System;

namespace Bookstore.Domains.People
{
    public class Result : IResult
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Error { get; set; }
        public string Warning { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}