using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace NewsSearch.Core.Sources
{
    public class GuardianSearch : BaseSearch
    {
        // TODO: create a factory to create the Search Entities
        public GuardianSearch()
            : base((int)EnumSources.TheGuardian, 
                "http://content.guardianapis.com/", 
                "search?q={0}&api-key=jhn82w8ge5n86jvghm4ud6tm", 
                "The Guardian")
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