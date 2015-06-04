using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace NewsSearch.Core.Sources
{
    public class YouTubeSearch : BaseSearch<YouTubeResult>
    {
        public YouTubeSearch()
            : base(true,
                "https://www.googleapis.com/youtube/v3/",
                "search?safeSearch=moderate&order=relevance&part=snippet&q={0}&relevanceLanguage=en&maxResults=10&key=AIzaSyBTi_oeX4kZBmtF3lLbVhcjimXCTnvIt_E",
                "YouTube")
        {
        }

        public YouTubeSearch(bool lazyLoading, string apiBaseAddress, string apiQueryString, string sourceName) : base(lazyLoading, apiBaseAddress, apiQueryString, sourceName)
        {
        }

        protected override void LoadResponse()
        {
            if (ApiResponse == null || !ApiResponse.ContainsKey("items"))
                return;

            var response = new Dictionary<string, object>(ApiResponse, StringComparer.InvariantCultureIgnoreCase);

            Mapper.Map(response, this);
        }
    }

    public class YouTubeResult : BaseResult
    {
        
    }
}