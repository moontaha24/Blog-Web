using System;
using Blog.Web.Data;
using Blog.Web.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Controllers
{
    public class AdminBlogPostController : Controller
    {
        private readonly BlogDbContext _dbContext;

        public AdminBlogPostController(BlogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string searchTerm, string sortBy, int page = 1, int pageSize = 5)
        {
            ViewData["ActivePage"] = "MyBlog";
            ViewData["SearchTerm"] = searchTerm;
            var query = string.IsNullOrEmpty(searchTerm)
                ? _dbContext.BlogPosts.AsQueryable()
                : _dbContext.BlogPosts
                    .Where(bp => bp.Heading.Contains(searchTerm) ||
                                 bp.Content.Contains(searchTerm) ||
                                 bp.Author.Contains(searchTerm));

            switch (sortBy)
            {
                case "Title":
                    query = query.OrderBy(bp => bp.Heading);
                    break;
                case "Date":
                    query = query.OrderByDescending(bp => bp.PublishedDate);
                    break;
                case "Author":
                    query = query.OrderBy(bp => bp.Author);
                    break;
            }

            // Pagination: Calculate total items and apply page size
            var totalItems = query.Count();
            var blogPosts = query
                .Skip((page - 1) * pageSize) // Skip items for previous pages
                .Take(pageSize)             // Take only `pageSize` items
                .ToList();

            // Set pagination data for the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SortBy = sortBy;



            return View("IndexBlog", blogPosts);
        }


        public IActionResult Create()
        {
            ViewData["ActivePage"] = "AddBlogPost";
            return View();
        }



        [HttpPost]
        public IActionResult Create(BlogPost newPost, string tags)
        {
            if (ModelState.IsValid)
            {
                newPost.Id = Guid.NewGuid();
                newPost.PublishedDate = DateTime.Now;

                if (!string.IsNullOrEmpty(tags))
                {
                    var tagList = tags.Split(',').Select(tag => new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = tag.Trim(),
                        DisplayName = tag.Trim()
                    }).ToList();

                    newPost.Tags = tagList;
                }
                else
                {
                    ModelState.AddModelError("Tags", "The Tags field is required.");
                    return View(newPost);
                }
                _dbContext.BlogPosts.Add(newPost);
                _dbContext.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            return View(newPost);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var blogPost = _dbContext.BlogPosts.FirstOrDefault(bp => bp.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        [HttpPost]
        public IActionResult Edit(BlogPost updatedPost)
        {
            if (ModelState.IsValid)
            {
                var existingPost = _dbContext.BlogPosts.FirstOrDefault(bp => bp.Id == updatedPost.Id);

                if (existingPost != null)
                {
                    existingPost.Heading = updatedPost.Heading;
                    existingPost.PageTitle = updatedPost.PageTitle;
                    existingPost.Content = updatedPost.Content;
                    existingPost.ShortDescription = updatedPost.ShortDescription;
                    //existingPost.FeaturedImageUrl = updatedPost.FeaturedImageUrl;
                    existingPost.UrlHandle = updatedPost.UrlHandle;
                    existingPost.PublishedDate = updatedPost.PublishedDate;
                    existingPost.Author = updatedPost.Author;
                    existingPost.Visible = updatedPost.Visible;

                    _dbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(updatedPost);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var blogPost = _dbContext.BlogPosts.FirstOrDefault(bp => bp.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost); // This will display a confirmation page
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var blogPost = _dbContext.BlogPosts.FirstOrDefault(bp => bp.Id == id);

            if (blogPost != null)
            {
                _dbContext.BlogPosts.Remove(blogPost);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        [HttpGet]
        public IActionResult AdminDetails(Guid id)
        {
            var blogPost = _dbContext.BlogPosts
                .Include(bp => bp.Tags)
                .FirstOrDefault(bp => bp.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View("AdminDetails", blogPost);
        }



    }
}
