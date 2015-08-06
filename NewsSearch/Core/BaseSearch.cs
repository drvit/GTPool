using System;
using System.Collections.Generic;

namespace NewsSearch.Core
{
    public abstract class BaseSearch : ISearch
    {
        protected BaseSearch(bool lazyLoading, string apiBaseAddress, string apiQueryString, string sourceName)
        {
            LazyLoading = lazyLoading;
            ApiBaseAddress = apiBaseAddress;
            ApiQueryString = apiQueryString;
            SourceName = sourceName;

            _responseLoaded = false;
        }

        private bool _responseLoaded;

        public string ApiBaseAddress { get; private set; }
        public bool LazyLoading { get; private set; }
        public string Query { get; set; }

        private string _apiQueryString;
        public string ApiQueryString
        {
            get { return string.Format(_apiQueryString, Query); }
            private set { _apiQueryString = value; }
        }

        private Dictionary<string, object> _apiResponse;
        public Dictionary<string, object> ApiResponse
        {
            get { return _apiResponse; }
            set
            {
                _apiResponse = value;

                if (!LazyLoading && !_apiResponse.ContainsKey("error"))
                {
                    _responseLoaded = true;
                    LoadResponse();
                }
            }
        }

        protected abstract void LoadResponse();

        protected void LazyLoadResponse()
        {
            if (LazyLoading && !_responseLoaded)
            {
                _responseLoaded = true;
                LoadResponse();
            }
        }

        #region Search Properties
        public string SourceName { get; private set; }

        private string _status;
        public string Status
        {
            get
            {
                LazyLoadResponse();
                return _status;
            }
            set { _status = value; }
        }

        private int _total;
        public int Total
        {
            get
            {
                LazyLoadResponse();
                return _total;
            }
            set { _total = value; }
        }

        private int _startIndex;
        public int StartIndex
        {
            get
            {
                LazyLoadResponse();
                return _startIndex;
            }
            set { _startIndex = value; }
        }

        private int _pageSize;
        public int PageSize
        {
            get
            {
                LazyLoadResponse();
                return _pageSize;
            }
            set { _pageSize = value; }
        }

        private int _currentPage;
        public int CurrentPage
        {
            get
            {
                LazyLoadResponse();
                return _currentPage;
            }
            set { _currentPage = value; }
        }

        private int _pages;
        public int Pages
        {
            get
            {
                LazyLoadResponse();
                return _pages;
            }
            set { _pages = value; }
        }

        private string _orderBy;
        public string OrderBy
        {
            get
            {
                LazyLoadResponse();
                return _orderBy;
            }
            set { _orderBy = value; }
        }

        private IEnumerable<IResult> _results;
        public virtual IEnumerable<IResult> Results
        {
            get
            {
                LazyLoadResponse();
                return _results;
            }
            set { _results = value; }
        }

        #endregion
    }
}
