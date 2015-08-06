using System;
using System.Collections.Generic;
using AutoMapper;

namespace NewsSearch.Core.Sources
{
    public class SocialMentionSearch : BaseSearch
    {
        // TODO: create a factory to create the Search Entities
        public SocialMentionSearch()
            : base(true,
                "http://api2.socialmention.com/",
                "search?q={0}&f=json&lang=en&t=news",
                "Social Mention")
        {
        }

        public SocialMentionSearch(bool lazyLoading, string apiBaseAddress, string apiQueryString, string sourceName)
            : base(lazyLoading, apiBaseAddress, apiQueryString, sourceName)
        {
        }

        protected override void LoadResponse()
        {
            if (ApiResponse == null || !ApiResponse.ContainsKey("items"))
                return;

            var response = new Dictionary<string, object>(ApiResponse, StringComparer.InvariantCultureIgnoreCase);

            Mapper.Map(response, this);
        }

        public new IEnumerable<SocialMentionResult> Results
        {
            get { return (IEnumerable<SocialMentionResult>)base.Results; }
            set { base.Results = value; }
        }
    }

    public class SocialMentionResult : BaseResult
    {
    }
}