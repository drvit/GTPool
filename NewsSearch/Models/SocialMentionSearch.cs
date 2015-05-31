using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace NewsSearch.Models
{
    public class SocialMentionSearch : QueryableSource
    {
        // TODO: create a factory to create the Search Entities
        public SocialMentionSearch()
            : base(true,
                "http://socialmention.com/", 
                "search?q={0}", 
                "Social Mention")
        {
        }

        public SocialMentionSearch(bool lazyLoading, string apiBaseAddress, string apiQueryString, string sourceName) 
            : base(lazyLoading, apiBaseAddress, apiQueryString, sourceName)
        {
        }

        protected override void LoadResponse()
        {
            if (ApiResponse == null || !ApiResponse.ContainsKey("response"))
                return;

            var response = new Dictionary<string, object>((Dictionary<string, object>)ApiResponse["response"],
                StringComparer.InvariantCultureIgnoreCase);

            Mapper.Map(response, this);
        }
    }

    //public class SocialMentionSearchResult : SourceResult
    //{

    //}
}