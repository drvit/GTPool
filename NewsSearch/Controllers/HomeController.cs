using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GTPool;
using NewsSearch.Core;
using NewsSearch.Core.Services;
using NewsSearch.Core.Sources;
using NewsSearch.Models;
using GTP = GTPool.GenericThreadPool;
using System.Threading;
using NewsSearch.Infrastructure.Utils;

namespace NewsSearch.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var model = new SearchViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SearchViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sources = GetSources();

                var sessionId = HttpContext.Session != null
                    ? HttpContext.Session.SessionID
                    : Utils.GenerateUniqueNumber().ToString();

                GlobalCache.Remove(sessionId);

                Utils.Log((new SessionLog(System.Web.HttpContext.Current)).ToString());
                Utils.Log(string.Format(">>>> Search query: \"{0}\"", model.SearchQuery));

                foreach (var src in sources)
                {
                    GTP.AddJob(new ManagedJob(
                        work: (Func<ISearch, string, ISearch>) ApiHelper.Execute,
                        parameters: new object[] {src, model.SearchQuery},
                        callback: ((Action<string, ISearch>) ((token, s) =>
                        {
                            Utils.Log(string.Format("{0} returned results with status code {1}", 
                                s.SourceName, s.ResponseStatusCode.ToString()));

                            s.SearchStatus = EnumSearchStatus.Completed;
                            GlobalCache.Add(token, s.Id.ToString(), s);
                        })),
                        callbackParameters: new object[] {sessionId},
                        onError: (ex =>
                        {
                            Utils.Log(string.Format("{0}. Inner Exception: {1}", ex.Message, ex.InnerException.Message), true);

                            if (ex.JobParameters == null) 
                                return;

                            var exSource = ex.JobParameters[0] as ISearch 
                                           ?? ex.JobParameters[1] as ISearch;

                            if (exSource == null) 
                                return;

                            exSource.SearchStatus = EnumSearchStatus.Completed;
                            exSource.LoadError(new Dictionary<string, object>
                                (StringComparer.InvariantCultureIgnoreCase)
                            {
                                {"error", ex.InnerException}
                            });

                            GlobalCache.Add(sessionId, exSource.Id.ToString(), exSource);
                        })));
                }

                model.SearchResults = sources;
                return View(model);
            }

            return RedirectToAction("Index");
        }

        private static List<ISearch> GetSources()
        {
            var sources = new List<ISearch>
            {
                new WikipediaSearch("https://en.wikipedia.org/", "w/api.php?action=query&prop=extracts|info&format=json&exchars=400&exlimit=1&explaintext=&exsectionformat=plain&inprop=url%7Cdisplaytitle&rawcontinue=&titles=iron%20maiden&generator=search&gsrprop=snippet&gsroffset=0&gsrlimit=1&gsrsearch={0}"),
                new GuardianSearch("http://content.guardianapis.com/", "search?q={0}&api-key=jhn82w8ge5n86jvghm4ud6tm"),
                new SocialMentionSearch("http://api2.socialmention.com/", "search?q={0}&f=json&lang=en&t=news"),
                new YouTubeSearch("https://www.googleapis.com/youtube/v3/", "search?safeSearch=moderate&order=relevance&part=snippet&q={0}&relevanceLanguage=en&maxResults=10&key=AIzaSyBTi_oeX4kZBmtF3lLbVhcjimXCTnvIt_E"),
                new RedditSearch("http://www.reddit.com/", "search.json?q={0}")
            };

            return sources;
        }

        [HttpGet]
        public PartialViewResult GetSourceResult(int id)
        {
            try
            {
                if (Session == null || string.IsNullOrEmpty(Session.SessionID))
                    return null;

                var source = GlobalCache.GetOnce(Session.SessionID, id.ToString()) as ISearch;
                if (source == null || source.SearchStatus != EnumSearchStatus.Completed)
                    return null;

                if (source.Results != null && source.Results.Any())
                {
                    return PartialView((EnumSources) id == EnumSources.Wikipedia 
                        ? "_WikipediaResult" : "_SourceResult", source);
                }

                ViewBag.ResponseText = "Nothing returned from this source!";
            }
            catch (Exception ex)
            {
                Utils.Log(string.Format("GetSourceResult Exception: {0} inner: {1}", ex.Message,
                    (ex.InnerException != null ? ex.InnerException.Message : "")));

                ViewBag.ResponseText = "Failed to get results from this source. Please, try again.";
            }

            return PartialView("_NoResults");
        }

        public ActionResult Error()
        {
            return RedirectToAction("Index");
        }
    }
}