using System.Collections.Generic;

namespace NewsSearch.Core
{
    public interface ISearch
    {
        string SourceName { get; }
        string ApiBaseAddress { get; }
        string ApiQueryString { get; }
        string Query { get; set; }
        Dictionary<string, object> ApiResponse { get; set; }
        IEnumerable<IResult> Results { get; set; }
    }
}
