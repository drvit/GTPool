using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsSearch.Core;
using NewsSearch.Core.Services;
using NewsSearch.Core.Sources;
using NewsSearch.Models;

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
            var searchResults = new List<Tuple<ISearch, IEnumerable<BaseResult>>>();
            var guardian = new GuardianSearch();
            var socialMention = new SocialMentionSearch();
            var youtube = new YouTubeSearch();

            ApiHelper.Execute(guardian, model.SearchQuery);
            ApiHelper.Execute(socialMention, model.SearchQuery);
            ApiHelper.Execute(youtube, model.SearchQuery);

            searchResults.Add(new Tuple<ISearch, IEnumerable<BaseResult>>(guardian, guardian.Results));
            searchResults.Add(new Tuple<ISearch, IEnumerable<BaseResult>>(socialMention, socialMention.Results));
            searchResults.Add(new Tuple<ISearch, IEnumerable<BaseResult>>(youtube, youtube.Results));

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