using System.Diagnostics;
using Blog.Web.Data;
using Blog.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BlogDbContext _dbContext; // Injected DbContext

        public HomeController(ILogger<HomeController> logger, BlogDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext; // Assign the injected DbContext
        }

        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Home";
            // Fetch the latest 3 blog posts sorted by PublishedDate
            var latestPosts = _dbContext.BlogPosts
                .OrderByDescending(bp => bp.PublishedDate)
                .Take(3)
                .ToList();

            return View(latestPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Details(Guid id)
        {
           
            var post = _dbContext.BlogPosts.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);


        }

    }
}
