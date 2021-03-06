﻿using System;
using System.Collections.Generic;
using System.Linq;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Core.Sources
{
    public class WikipediaSearch : BaseSearch
    {
        public WikipediaSearch(string apiBaseAddress, string apiQueryString)
            : base((int)EnumSources.Wikipedia, apiBaseAddress,
            apiQueryString, EnumSources.Wikipedia.ToDescription())
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