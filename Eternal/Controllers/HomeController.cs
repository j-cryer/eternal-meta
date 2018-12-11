using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eternal.Models;
using Eternal.Models.ViewModels;
using Eternal.Utility;

namespace Eternal.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            FeaturedContent featuredContent = new FeaturedContent
            {
                FeaturedCards = await DbHelper.GetFeaturedCards(),
                FeaturedDecks = await DbHelper.GetFeaturedDecks()
            };

            return View(featuredContent);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
