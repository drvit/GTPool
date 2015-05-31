using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewsSearch.Models
{
    //public interface ISourceEntity<TResult>
    //    where TResult : IResult
    public interface ISourceEntity
    {
        string ApiBaseAddress { get; }
        string ApiQueryString { get; }
        string Query { get; set; }
        Dictionary<string, object> ApiResponse { get; set; }
        //IList<TResult> Results { get; set; }
    }
}
