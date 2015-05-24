using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsSearch.Models
{
    public class SearchNewsViewModel
    {
        public string SearchQuery { get; set; }
        public Dictionary<string, string> Results { get; set; } 
    }
}