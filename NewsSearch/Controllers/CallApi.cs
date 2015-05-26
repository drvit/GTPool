using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using NewsSearch.Models;

namespace NewsSearch.Controllers
{
    public static class CallApi
    {
        public static async Task Execute(string query)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://content.guardianapis.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                query = HttpUtility.UrlEncode(query);

                // New code:
                var response = await client.GetAsync("search?q=" + query + "&api-key=jhn82w8ge5n86jvghm4ud6tm");
                if (response.IsSuccessStatusCode)
                {
                    var searchResult = await response.Content.ReadAsAsync<GuardianSearchResult>();
                }
            }
        }
    }
}