using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using NewsSearch.Models;
using AutoMapper;

namespace NewsSearch.Controllers
{
    public static class ApiHelper
    {
        public static void Execute<T>(T searchEntity, string query)
            where T : class, ISourceEntity
        {
            if (string.IsNullOrEmpty(query))
                return;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(searchEntity.ApiBaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                searchEntity.Query = HttpUtility.UrlEncode(query);
                var response = client.GetAsync(searchEntity.ApiQueryString).Result;

                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var jsonSerializer = new JavaScriptSerializer();
                    searchEntity.ApiResponse = jsonSerializer.DeserializeObject(response.Content.ReadAsStringAsync().Result) as Dictionary<string, object>;
                }
            }
        }
    }
}