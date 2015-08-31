using System;
using System.Collections.Generic;
using System.Linq;
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
                var sources = new List<ISearch>
                {
                    new WikipediaSearch(),
                    new GuardianSearch(),
                    new SocialMentionSearch(),
                    new YouTubeSearch(),
                    new RedditSearch()
                };

                var sessionId = HttpContext.Session != null
                    ? HttpContext.Session.SessionID
                    : Utils.GenerateUniqueNumber().ToString();

                GlobalCache.Remove(sessionId);

                foreach (var src in sources)
                {
                    GTP.AddJob(new ManagedJob(
                        work: (Func<ISearch, string, ISearch>) ApiHelper.Execute,
                        parameters: new object[] {src, model.SearchQuery},
                        callback: ((Action<string, ISearch>) ((token, s) =>
                        {
                            s.SearchStatus = EnumSearchStatus.Completed;
                            GlobalCache.Add(token, s.Id.ToString(), s);
                        })),
                        callbackParameters: new object[] {sessionId},
                        onError: (ex =>
                        {
                            var source = (ISearch) ex.JobParameters[0];

                            sources.First(x => x.SourceName == source.SourceName)
                                .LoadError(new Dictionary<string, object>
                                    (StringComparer.InvariantCultureIgnoreCase)
                                {
                                    {"error", ex.InnerException}
                                });
                        })));
                }

                model.SearchResults = sources;
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public PartialViewResult GetSourceResult(int id)
        {
            if (Session != null)
            {
                var token = Session.SessionID;
                if (!string.IsNullOrEmpty(token))
                {
                    var source = GlobalCache.Get(token, id.ToString());
                    if (source != null)
                    {
                        var src = (ISearch) source;
                        if (src.SearchStatus == EnumSearchStatus.Completed 
                            && src.Results != null && src.Results.Any())
                        {
                            GlobalCache.Remove(token, id.ToString());

                            if ((EnumSources) id == EnumSources.Wikipedia)
                            {
                                return PartialView("_WikipediaResult", (ISearch) source);
                            }

                            return PartialView("_SourceResult", (ISearch) source);
                        }

                        return PartialView("_NoResults");
                    }
                }
            }

            return null;
        }

        public ActionResult Error()
        {
            return RedirectToAction("Index");
        }
    }
}