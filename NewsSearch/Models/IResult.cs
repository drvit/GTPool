using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewsSearch.Models
{
    public interface IResult
    {
        string SectionId { get; set; }
        string SectionName { get; set; }
        string Title { get; set; }
        DateTime PublicationDate { get; set; }
        string Id { get; set; }
        string WebUrl { get; set; }
        string ApiUrl { get; set; }
    }
}
