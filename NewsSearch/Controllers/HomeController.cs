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
            var sources = new List<ISearch>
            {
                new WikipediaSearch(),
                new GuardianSearch(),
                new SocialMentionSearch(),
                new YouTubeSearch()
            };

            using (var gtp = GTP.Init<GtpSync>(4))
            {
                foreach (var src in sources)
                {
                    gtp.AddJob(new ManagedSyncJob(
                        (Action<ISearch, string>)ApiHelper.Execute,
                        new object[] { src, model.SearchQuery },
                        (ex =>
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
            }

            if (ModelState.IsValid)
            {
                model.SearchResults = sources;
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Error()
        {
            return RedirectToAction("Index");
        }
    }
}