using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsSearch.Models
{
    [Serializable]
    public class SearchResponse
    {
        public GuardianSearchResult Response { get; set; }
    }

    [Serializable]
    public class GuardianSearchResult : ISearchResponse
    {
        public GuardianSearchResult() { }

        public void LoadResults(IDictionary<string, object> results)
        {
        }

        public string Status { get; set; }
        public int Total { get; set; }
        public int StartIndex { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int Pages { get; set; }
        public string OrderBy { get; set; }
        public List<GuardianResult> Results { get; set; }
    }

    [Serializable]
    public class GuardianResult : IResult
    {
        public string SectionId { get; set; }
        public string WebTitle { get; set; }
        public DateTime WebPublicationDate { get; set; }
        public string Id { get; set; }
        public string WebUrl { get; set; }
        public string ApiUrl { get; set; }
        public string SectionName { get; set; }
    }
}