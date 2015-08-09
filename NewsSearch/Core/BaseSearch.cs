using System;
using System.Collections.Generic;
using System.Linq;
using GTPool;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Core
{
    public abstract class BaseSearch : ISearch
    {
        protected BaseSearch(string apiBaseAddress, string apiQueryString, string sourceName)
        {
            ApiBaseAddress = apiBaseAddress;
            ApiQueryString = apiQueryString;
            SourceName = sourceName;

            HeaderMapping = new Dictionary<string, Tuple<SearchFields, Func<string, object, object>>>();
            ResultMapping = new Dictionary<string, Tuple<ResultFields, Func<string, object, object>>>();
        }

        public string ApiBaseAddress { get; private set; }
        public string Query { get; set; }

        private string _apiQueryString;
        public string ApiQueryString
        {
            get { return string.Format(_apiQueryString, Query); }
            private set { _apiQueryString = value; }
        }

        public IDictionary<string, Tuple<SearchFields, Func<string, object, object>>> HeaderMapping { get; private set; }
        public IDictionary<string, Tuple<ResultFields, Func<string, object, object>>> ResultMapping { get; private set; }

        public void AddHeaderMappingItem(string sourceField, SearchFields targetField,
            Func<string, object, object> valueFormater)
        {
            if (!HeaderMapping.ContainsKey(sourceField))
                HeaderMapping.Add(sourceField, new Tuple<SearchFields, Func<string, object, object>>(targetField, valueFormater));
        }

        public void AddResultMappingItem(string sourceField, ResultFields targetField,
            Func<string, object, object> valueFormater)
        {
            if (!ResultMapping.ContainsKey(sourceField))
                ResultMapping.Add(sourceField, new Tuple<ResultFields, Func<string, object, object>>(targetField, valueFormater));
        }

        public abstract void LoadResponse(Dictionary<string, object> response);

        public void LoadError(Dictionary<string, object> error)
        {
            AddHeaderMappingItem("Error", SearchFields.Error, null);
            PopulateFields(error);
        }

        protected void PopulateFields(Dictionary<string, object> response)
        {
            if (HeaderMapping.Count <= 0 || response == null)
                return;

            foreach (var header in HeaderMapping)
            {
                if (header.Value.Item1 != SearchFields.Results)
                {
                    object value;
                    if (TryGetSourceFieldValue(response, header.Key, out value))
                    {
                        var formatedValue = header.Value.Item2 != null
                            ? header.Value.Item2(header.Value.Item1.ToString(), value)
                            : value;

                        if (!TrySetProperyValue(this, header.Value.Item1.ToString(), formatedValue))
                        {
                            Utils.Log(string.Format("News Search Site: Failed to load {0} property value {1}", 
                                SourceName, header.Value.Item1));
                        }
                    }
                }
                else if (response.ContainsKey(header.Key))
                {
                    var results = (
                        from resp in (object[])response[header.Key]
                        select new Dictionary<string, object>((Dictionary<string, object>)resp,
                            StringComparer.InvariantCultureIgnoreCase)).ToList();

                    PopulateResults(results);
                }
            }
        }

        private void PopulateResults(IEnumerable<Dictionary<string, object>> responseResults)
        {
            if (ResultMapping.Count <= 0 || responseResults == null)
                return;

            var res = new List<IResult>();

            foreach (var searchResult in responseResults)
            {
                var newResult = new Result();

                foreach (var result in ResultMapping)
                {
                    object value;
                    if (TryGetSourceFieldValue(searchResult, result.Key, out value))
                    {
                        var formatedValue = result.Value.Item2 != null
                            ? result.Value.Item2(result.Value.Item1.ToString(), value)
                            : value;

                        TrySetProperyValue(newResult, result.Value.Item1.ToString(), formatedValue);
                    }
                }

                res.Add(newResult);
            }
            
            Results = res;
        }

        private bool TrySetProperyValue(object instance, string fieldName, object formatedValue)
        {
            try
            {
                DynamicUtils.SetPropertyValue(instance, fieldName, formatedValue);
                return true;
            }
            catch (Exception ex)
            {
                Utils.Log(string.Format("ERROR | News Search Site: Failed to load {0} property value {1}. /n With Exception: {2}. {3}",
                    SourceName, fieldName, ex.Message, (ex.InnerException != null ? "/n Inner Exception: " + ex.InnerException : string.Empty)));

                return false;
            }
        }

        private static bool TryGetSourceFieldValue(IReadOnlyDictionary<string, object> response,
            string mappingFieldName, out object value)
        {
            value = null;

            try
            {
                var node = response;
                var sourceFieldName = mappingFieldName.Split('.');
                var index = 0;

                if (sourceFieldName.Length == 2)
                {
                    object innerOjbects;
                    if (response.TryGetValue(sourceFieldName[0], out innerOjbects))
                    {
                        node = new Dictionary<string, object>((Dictionary<string, object>) innerOjbects,
                            StringComparer.InvariantCultureIgnoreCase);

                        index++;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (node.TryGetValue(sourceFieldName[index], out value))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        #region Search Properties

        public string SourceName { get; private set; }

        public string Status { get; set; }

        public int Total { get; set; }

        public int StartIndex { get; set; }

        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        public int Pages { get; set; }

        public string OrderBy { get; set; }

        public IEnumerable<IResult> Results { get; set; }

        public Exception Error { get; set; }

        #endregion

        #region Public Parse Methods

        protected static object DateTimeParseStringUtc(string targetFieldName, object value)
        {
            //6/24/2013 10:07:04 AM
            DateTime dt;
            if (value != null && DateTime.TryParse(value.ToString(), out dt))
                return dt;

            return DateTime.Now;
        }

        protected static object IntParseString(string targetFieldName, object value)
        {
            int ret;
            if (value != null && int.TryParse(value.ToString(), out ret))
                return ret;

            return 0;
        }

        protected static string FormatLink(string targetFieldName, object value)
        {
            var ret = string.Empty;

            if (value != null && !value.ToString().Contains("http"))
                ret = string.Format("http://{0}", value);

            return ret;
        }

        protected static object DateTimeParseLong(string targetFieldName, object value)
        {
            //6/24/2013 10:07:04 AM
            long ts;
            if (value != null && long.TryParse(value.ToString(), out ts))
                return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(ts).ToLocalTime();

            return DateTime.Now;
        }

        protected static object StringParseInt(string targetFieldName, object value)
        {
            if (value != null)
                return value.ToString();

            return string.Empty;
        }

        #endregion
    }
}
