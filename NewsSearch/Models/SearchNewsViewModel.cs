using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsSearch.Models
{
    [Serializable]
    public class SearchNewsViewModel
    {
        public string SearchQuery { get; set; }
        public IList<NewsResults> Results { get; set; } 
    }

    [Serializable]
    public class NewsResults
    {
        public string Source { get; set; }
        public string Headline { get; set; }
        public string Content { get; set; }
    }
}