
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsSearch.Models.Tests
{
    class Main
    {
        Main()
        {
            var guardianSearch = new GuardianSearch();
            var socialMentionSearch = new SocialMentionSearch();

            var model = new SearchViewModel();
            model.SourceResults.Add(guardianSearch.GetResults());
            model.SourceResults.Add(socialMentionSearch.GetResults());
        }
    }

    class SearchViewModel
    {
        public SearchViewModel()
        {
            SourceResults = new List<Search<Result>>();
        }

        public IList<Search<Result>> SourceResults { get; set; }
    }

    interface IResult
    {
        string Title { get; set; }
        DateTime DatePublished { get; set; }
    }

    class Result : IResult
    {
        public string Title { get; set; }
        public DateTime DatePublished { get; set; }
    }

    interface ISearch<TResult> where TResult : IResult
    {
        string SourceName { get; set; }
        string Query { get; set; }
        string ApiUrl { get; set; }
        Dictionary<string, object> ApiResponse { get; set; }
        IList<TResult> Results { get; set; }
    }

    internal abstract class BaseSearch<TResult> : ISearch<TResult>
        where TResult : class, IResult
    {
        public string SourceName { get; set; }
        public string Query { get; set; }
        public string ApiUrl { get; set; }
        public Dictionary<string, object> ApiResponse { get; set; }
        public IList<TResult> Results { get; set; }
        public abstract void LoadProperties();
    }

    class Search<TResult> : BaseSearch<TResult>
        where TResult : class, IResult
    {
        public Search<Result> GetResults()
        {
            var search = new Search<Result>
            {
                SourceName = SourceName,
                Query = Query,
                ApiUrl = ApiUrl,
                ApiResponse = ApiResponse,
                Results = Results as List<Result>
            };

            return search;
        }

        public override void LoadProperties()
        {
        }
    }

    class GuardianSearch : Search<GuardianResult>
    {
    }

    class GuardianResult : Result
    {
    }

    class SocialMentionSearch : Search<SocialMentionResult>
    {
    }

    class SocialMentionResult : Result
    {
    }

}