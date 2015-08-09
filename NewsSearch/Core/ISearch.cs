using System;
using System.Collections.Generic;

namespace NewsSearch.Core
{
    public interface ISearch
    {
        string SourceName { get; }
        string ApiBaseAddress { get; }
        string ApiQueryString { get; }
        string Query { get; set; }
        string Status { get; set; }
        int Total { get; set; }
        int StartIndex { get; set; }
        int PageSize { get; set; }
        int CurrentPage { get; set; }
        int Pages { get; set; }
        string OrderBy { get; set; }
        void LoadResponse(Dictionary<string, object> apiResponse);
        void LoadError(Dictionary<string, object> error);
        IEnumerable<IResult> Results { get; set; }
        Exception Error { get; set; }
    }

    public enum SearchFields
    {
        SourceName,
        ApiBaseAddress,
        ApiQueryString,
        Query,
        Status,
        Total,
        StartIndex,
        PageSize,
        CurrentPage,
        Pages,
        OrderBy,
        Results,
        Error
    }
}
