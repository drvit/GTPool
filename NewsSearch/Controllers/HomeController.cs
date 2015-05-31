using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsSearch.Models;

namespace NewsSearch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new SearchNewsViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SearchNewsViewModel model)
        {
            //var searchResults = new List<QueryableSource<IResult>>();
            var searchResults = new List<QueryableSource>();
            var guardian = new GuardianSearch();
            //var socialMention = new SocialMentionSearch();

            ApiHelper.Execute(guardian, model.SearchQuery);
            //ApiHelper.Execute(socialMention, model.SearchQuery);

            searchResults.Add(guardian);
            //searchResults.Add(socialMention);

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