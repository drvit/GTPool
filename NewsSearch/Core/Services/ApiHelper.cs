using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Script.Serialization;

namespace NewsSearch.Core.Services
{
    public static class ApiHelper
    {
        public static void Execute<T>(T search, string query)
            where T : class, ISearch
        {
            if (string.IsNullOrEmpty(query))
                return;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(search.ApiBaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                search.Query = HttpUtility.UrlEncode(query);
                var response = client.GetAsync(search.ApiQueryString).Result;

                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(result) && result.Contains("{"))
                {
                    var jsonSerializer = new JavaScriptSerializer();

                    search.LoadResponse(jsonSerializer.DeserializeObject(result) as Dictionary<string, object>);
                }
            }
        }
    }
}