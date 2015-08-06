using System;
using System.Collections.Generic;
using NewsSearch.Core;

namespace NewsSearch.Models
{
    public class SearchViewModel
    {
        public string SearchQuery { get; set; }
        public IList<Tuple<ISearch, IEnumerable<IResult>>> SearchResults { get; set; } 
    }
}