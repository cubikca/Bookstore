using System;

namespace Bookstore.Domains.People
{
    public interface IResult
    {
        int Status { get; set; }
        string Error { get; set; }
        string Warning { get; set; }
        string Message { get; set; }
        Exception Exception { get; set; }
    }
}