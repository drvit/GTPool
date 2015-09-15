using System;
using System.Collections.Generic;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Core.Sources
{
    public class SocialMentionSearch : BaseSearch
    {
        public SocialMentionSearch(string apiBaseAddress, string apiQueryString)
            : base((int)EnumSources.SocialMention, apiBaseAddress,
            apiQueryString, EnumSources.SocialMention.ToDescription())
        { }

        public override void LoadResponse(Dictionary<string, object> apiResponse)
        {
            if (apiResponse == null || !apiResponse.ContainsKey("items"))
                return;

            var response = new Dictionary<string, object>(apiResponse, StringComparer.InvariantCultureIgnoreCase);

            AddHeaderMappingItem("Count", SearchFields.Total, IntParseString);
            AddHeaderMappingItem("items", SearchFields.Results, null);

            AddResultMappingItem("Title", ResultFields.Title, null);
            AddResultMappingItem("Description", ResultFields.Description, null);
            AddResultMappingItem("timestamp", ResultFields.PublicationDate, DateTimeParseLong);
            AddResultMappingItem("Id", ResultFields.Id, null);
            AddResultMappingItem("link", ResultFields.WebUrl, null);
            AddResultMappingItem("type", ResultFields.SectionName, null);
            AddResultMappingItem("domain", ResultFields.SubSourceName, null);
            AddResultMappingItem("favicon", ResultFields.SubSourceFavIcon, null);
            AddResultMappingItem("domain", ResultFields.SubSourceDomain, FormatLink);
            AddResultMappingItem("Embeded", ResultFields.Embeded, null);
            AddResultMappingItem("Language", ResultFields.Language, null);
            AddResultMappingItem("user", ResultFields.UserPublisher, null);
            AddResultMappingItem("user_image", ResultFields.UserImage, null);
            AddResultMappingItem("user_link", ResultFields.UserLink, null);
            AddResultMappingItem("geo", ResultFields.Geolocation, null);

            PopulateFields(response);
        }
    }
}