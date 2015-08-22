using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AutoMapper;
using NewsSearch.Infrastructure;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Core.Sources
{
    public class WikipediaSearch : BaseSearch
    {
        public WikipediaSearch()
            : base("https://en.wikipedia.org",
                "/w/api.php?action=query&prop=extracts|info&format=json&exchars=400&exlimit=1&explaintext=&exsectionformat=plain&inprop=url%7Cdisplaytitle&rawcontinue=&titles=iron%20maiden&generator=search&gsrprop=snippet&gsroffset=0&gsrlimit=1&gsrsearch={0}",
                "Wikipedia")
        { }

        public override void LoadResponse(Dictionary<string, object> apiResponse)
        {
            if (apiResponse == null || !apiResponse.ContainsKey("query"))
                return;

            var response =
                new Dictionary<string, object>(
                    (Dictionary<string, object>) ((Dictionary<string, object>) apiResponse["query"])["pages"],
                    StringComparer.InvariantCultureIgnoreCase);

            if (response.Any())
            {
                response.Add("Results", new[] {response[response.First().Key]});
                response.Remove(response.First().Key);

                AddHeaderMappingItem("Results", SearchFields.Results, null);

                AddResultMappingItem("displaytitle", ResultFields.Title, null);
                AddResultMappingItem("extract", ResultFields.Extract, null);
                AddResultMappingItem("touched", ResultFields.PublicationDate, DateTimeParseStringUtc);
                AddResultMappingItem("pageid", ResultFields.Id, StringParseInt);
                AddResultMappingItem("fullurl", ResultFields.WebUrl, null);
                AddResultMappingItem("canonicalurl", ResultFields.ApiUrl, null);

                PopulateFields(response);
            }
        }
    }
}