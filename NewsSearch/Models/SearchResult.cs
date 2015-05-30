using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsSearch.Models
{
    public class SourceResult : IResult
    {
        public string SectionId { get; set; }
        public string SectionName { get; set; }
        public string Title { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Id { get; set; }
        public string WebUrl { get; set; }
        public string ApiUrl { get; set; }
    }
}