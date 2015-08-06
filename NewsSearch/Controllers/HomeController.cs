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
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new SearchViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SearchViewModel model)
        {
            var searchResults = new List<Tuple<ISearch, IEnumerable<IResult>>>();

            var sources = new List<ISearch>
            {
                new GuardianSearch(),
                new SocialMentionSearch(),
                new YouTubeSearch()
            };

            using (var gtp = GTP.Init<GtpSync>(3))
            {
                foreach (var src in sources)
                {
                    var src1 = src;

                    gtp.AddJob(new ManagedSyncJob(
                        (Action<ISearch, string>)ApiHelper.Execute, new object[] { src1, model.SearchQuery },
                        (Action<ISearch>)(source =>
                        {
                            searchResults.Add(new Tuple<ISearch, IEnumerable<IResult>>(source, source.Results));
                        }), new object[] { src1 },
                        (ex =>
                        {
                            src1.ApiResponse = new Dictionary<string, object>
                            {
                                {"error", ex}
                            };

                            searchResults.Add(new Tuple<ISearch, IEnumerable<IResult>>(src1, src1.Results));
                        })));
                }
            }

            if (ModelState.IsValid)
            {
                model.SearchResults = searchResults;
                return View(model);
            }

            return RedirectToAction("Index");
        }

        
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}