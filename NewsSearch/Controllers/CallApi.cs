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
    public static class CallApi
    {
        public static void Execute<T>(T searchEntity, string query)
            where T : QueriableSource
        {
            if (string.IsNullOrEmpty(query))
                return;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(searchEntity.ApiBaseAddress); // new Uri("http://content.guardianapis.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //query = HttpUtility.UrlEncode(query);

                // New code:
                //var response = await client.GetAsync("search?q=" + query + "&api-key=jhn82w8ge5n86jvghm4ud6tm");
                //var response = client.GetAsync("search?q=" + query + "&api-key=jhn82w8ge5n86jvghm4ud6tm").Result;

                searchEntity.Query = HttpUtility.UrlEncode(query);
                var response = client.GetAsync(searchEntity.ApiQueryString).Result;

                //var response = client.GetStringAsync("search?q=" + query + "&api-key=jhn82w8ge5n86jvghm4ud6tm").Result;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    //var searchResult = await response.Content.ReadAsAsync<GuardianSearchResult>();
                    //searchResult = response.Content.ReadAsAsync<SearchResponse>().Result;

                    var jsonSerializer = new JavaScriptSerializer();
                    //searchResult =
                    //    jsonSerializer.Deserialize<SearchResponse>(response.Content.ReadAsStringAsync().Result);


                    searchEntity.ApiResponse = jsonSerializer.DeserializeObject(response.Content.ReadAsStringAsync().Result) as Dictionary<string, object>;

                    //if (result != null && result.ContainsKey("response"))
                    //{
                    //    var temp = new Dictionary<string, object>((Dictionary<string, object>) result["response"],
                    //        StringComparer.InvariantCultureIgnoreCase);

                    //    searchResult = Mapper.Map<Dictionary<string, object>, GuardianSearchResult>(temp);
                    //}
                }
            }
        }
    }
}