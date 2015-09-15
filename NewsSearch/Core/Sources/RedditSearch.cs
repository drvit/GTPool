using System;
using System.Collections.Generic;
using System.Linq;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Core.Sources
{
    public class RedditSearch : BaseSearch
    {
        public RedditSearch(string apiBaseAddress, string apiQueryString)
            : base((int)EnumSources.Reddit, apiBaseAddress,
            apiQueryString, EnumSources.Reddit.ToDescription())
        { }

        public override void LoadResponse(Dictionary<string, object> apiResponse)
        {
            if (apiResponse == null || !apiResponse.ContainsKey("data"))
                return;

            var children = (object[]) ((Dictionary<string, object>) apiResponse["data"])["children"];

            var data =
                children.Select(
                    child =>
                        new Dictionary<string, object>((Dictionary<string, object>) child,
                            StringComparer.InvariantCultureIgnoreCase))
                    .Select(res => (Dictionary<string, object>) res["data"])
                    .ToArray();

            var response = new Dictionary<string, object> {{"data", data}};

            AddHeaderMappingItem("data", SearchFields.Results, null);

            AddResultMappingItem("title", ResultFields.Title, null);
            AddResultMappingItem("author_[1]", ResultFields.UserPublisher, null);
            AddResultMappingItem("author_[2]", ResultFields.SubSourceName, null);
            AddResultMappingItem("author_[3]", ResultFields.SubSourceDomain, RedditFormatSubSourceLink);
            AddResultMappingItem("edited", ResultFields.PublicationDate, DateTimeParseStringUtc);
            AddResultMappingItem("id", ResultFields.Id, null);
            AddResultMappingItem("url", ResultFields.WebUrl, null);

            PopulateFields(response);
        }

        private static string RedditFormatSubSourceLink(string targetField, object value)
        {
            var ret = string.Empty;

            if (value != null && !string.IsNullOrEmpty(value.ToString()))
                ret = string.Format("https://www.reddit.com/user/{0}", value);

            return ret;
        }

    }
}