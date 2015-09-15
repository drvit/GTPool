using System;
using System.Collections.Generic;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Core.Sources
{
    public class YouTubeSearch : BaseSearch
    {
        public YouTubeSearch(string apiBaseAddress, string apiQueryString)
            : base((int)EnumSources.YouTube, apiBaseAddress,
            apiQueryString, EnumSources.YouTube.ToDescription())
        { }

        public override void LoadResponse(Dictionary<string, object> apiResponse)
        {
            if (apiResponse == null || !apiResponse.ContainsKey("items"))
                return;

            var response = new Dictionary<string, object>(apiResponse, StringComparer.InvariantCultureIgnoreCase);

            AddHeaderMappingItem("pageInfo.totalResults", SearchFields.Total, IntParseString);
            AddHeaderMappingItem("items", SearchFields.Results, null);

            AddResultMappingItem("snippet.title", ResultFields.Title, null);
            AddResultMappingItem("snippet.publishedAt", ResultFields.PublicationDate, DateTimeParseStringUtc);
            AddResultMappingItem("snippet.Description", ResultFields.Description, null);
            AddResultMappingItem("Id.videoId", ResultFields.WebUrl, YoutubeFormatLink);
            AddResultMappingItem("snippet.channelTitle", ResultFields.SubSourceName, null);
            AddResultMappingItem("thumbnails.default", ResultFields.SubSourceFavIcon, null);
            AddResultMappingItem("snippet.channelId", ResultFields.SubSourceDomain, YoutubeFormatDomainLink);

            PopulateFields(response);
        }

        private static string YoutubeFormatLink(string targetField, object value)
        {
            var ret = string.Empty;

            if (value != null && !string.IsNullOrEmpty(value.ToString()))
                ret = string.Format("https://www.youtube.com/watch?v={0}", value);

            return ret;
        }

        private static string YoutubeFormatDomainLink(string targetField, object link)
        {
            var ret = string.Empty;

            if (link != null && !string.IsNullOrEmpty(link.ToString()))
                ret = string.Format("https://www.youtube.com/channel/{0}", link);

            return ret;
        }

    }
}