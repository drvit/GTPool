using System;
using System.Collections.Generic;
using NewsSearch.Core;

namespace NewsSearch.Models
{
    public class SearchViewModel
    {
        public string SearchQuery { get; set; }
        public IList<ISearch> SearchResults { get; set; } 
    }
}