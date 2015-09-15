using System;
using System.Collections.Generic;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Core.Sources
{
    public class GuardianSearch : BaseSearch
    {
        public GuardianSearch(string apiBaseAddress, string apiQueryString)
            : base((int)EnumSources.TheGuardian, apiBaseAddress, 
            apiQueryString, EnumSources.TheGuardian.ToDescription())
        { }

        public override void LoadResponse(Dictionary<string, object> apiResponse)
        {
            if (apiResponse == null || !apiResponse.ContainsKey("response"))
                return;

            var response = new Dictionary<string, object>((Dictionary<string, object>)apiResponse["response"],
                StringComparer.InvariantCultureIgnoreCase);

            AddHeaderMappingItem("Status", SearchFields.Status, null);
            AddHeaderMappingItem("Total", SearchFields.Total, IntParseString);
            AddHeaderMappingItem("StartIndex", SearchFields.StartIndex, IntParseString);
            AddHeaderMappingItem("PageSize", SearchFields.PageSize, IntParseString);
            AddHeaderMappingItem("CurrentPage", SearchFields.CurrentPage, IntParseString);
            AddHeaderMappingItem("Pages", SearchFields.Pages, IntParseString);
            AddHeaderMappingItem("OrderBy", SearchFields.OrderBy, null);
            AddHeaderMappingItem("Results", SearchFields.Results, null);

            AddResultMappingItem("WebTitle", ResultFields.Title, null);
            AddResultMappingItem("SectionId", ResultFields.SectionId, null);
            AddResultMappingItem("WebPublicationDate", ResultFields.PublicationDate, DateTimeParseStringUtc);
            AddResultMappingItem("Id", ResultFields.Id, null);
            AddResultMappingItem("WebUrl", ResultFields.WebUrl, null);
            AddResultMappingItem("ApiUrl", ResultFields.ApiUrl, null);
            AddResultMappingItem("SectionName", ResultFields.SectionName, null);

            PopulateFields(response);
        }
    }
}