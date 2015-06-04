using System;
using System.Collections.Generic;

namespace Console
{
    internal interface IInterfaceA<T>
        where T : List<int>
    {
        T Get(T t);
    }

    class ClassA<T> : IInterfaceA<T> 
        where T : List<int>
    {
        public T Get(T t)
        {
            for (var i = 0; i < t.Count; i++)
                t[i] += 1;

            return t;
        }
    }

    class ClassB
    {
        ClassB()
        {

        }
    }



    class Main
    {

        Main()
        {
            var guardianSearch = new GuardianSearch();
            var socialMentionSearch = new SocialMentionSearch();

            var test = new List<Tuple<ISearch, IEnumerable<BaseResult>>>
            {
                new Tuple<ISearch, IEnumerable<BaseResult>>(guardianSearch, guardianSearch.Results),
                new Tuple<ISearch, IEnumerable<BaseResult>>(socialMentionSearch, socialMentionSearch.Results)
            };


        }
    }
    
    interface IResult
    {
        string Title { get; set; }
        DateTime DatePublished { get; set; }
    }

    abstract class BaseResult : IResult
    {
        public string Title { get; set; }
        public DateTime DatePublished { get; set; }
    }

    interface ISearch
    {
        string SourceName { get; set; }
        string Query { get; set; }
        string ApiUrl { get; set; }
        Dictionary<string, object> ApiResponse { get; set; }
    }

    abstract class BaseSearch<TResult> : ISearch
        where TResult : BaseResult
    {
        public string SourceName { get; set; }
        public string Query { get; set; }
        public string ApiUrl { get; set; }
        public Dictionary<string, object> ApiResponse { get; set; }
        public IEnumerable<TResult> Results { get; set; }
        protected abstract void LoadResponse();
    }

    class GuardianSearch : BaseSearch<GuardianResult>
    {
        protected override void LoadResponse()
        {
        }
    }

    class GuardianResult : BaseResult
    {
    }

    class SocialMentionSearch : BaseSearch<SocialMentionResult>
    {
        protected override void LoadResponse()
        {
        }
    }

    class SocialMentionResult : BaseResult
    {
    }

}