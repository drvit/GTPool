using System;
using System.Collections.Generic;
using AutoMapper;

namespace NewsSearch.Core.Sources
{
    public class GuardianSearch : BaseSearch
    {
        // TODO: create a factory to create the Search Entities
        public GuardianSearch()
            : base(true,
                "http://content.guardianapis.com/", 
                "search?q={0}&api-key=jhn82w8ge5n86jvghm4ud6tm", 
                "The Guardian")
        {
        }

        public GuardianSearch(bool lazyLoading, string apiBaseAddress, string apiQueryString, string sourceName)
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

        public new IEnumerable<GuardianResult> Results
        {
            get { return (IEnumerable<GuardianResult>) base.Results; }
            set { base.Results = value; }
        }
    }

    public class GuardianResult : BaseResult { }
}