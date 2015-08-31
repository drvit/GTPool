using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NewsSearch.Core;

namespace NewsSearch.Models
{
    public class SearchViewModel
    {
        [Required]
        public string SearchQuery { get; set; }
        public IList<ISearch> SearchResults { get; set; } 
    }
}