using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using MyWebFormApp.BLL;
using MyWebFormApp.BLL.DTOs;
using MyWebFormApp.BLL.Interfaces;

namespace SampleMVC.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleBLL _articleBLL;
        private readonly ICategoryBLL _categoryBLL;

        public ArticlesController(IArticleBLL articleBLL, ICategoryBLL categoryBLL)
        {
            _articleBLL = articleBLL;
            _categoryBLL = categoryBLL;
        }

        public IActionResult Index()
        {
            //Initialized Category for join with Article BLL
            List<ArticleDTO> articleDTO = new List<ArticleDTO>();
            var articles = _articleBLL.GetArticleWithCategory();
            foreach (var item in articles)
            {
                var categories = new List<CategoryDTO>();
                foreach (var cat in categories)
                {
                    categories.Add(new CategoryDTO
                    {
                        CategoryID = cat.CategoryID,
                        CategoryName = cat.CategoryName
                    });
                }
                articleDTO.Add(new ArticleDTO
                {
                    ArticleID = item.ArticleID,
                    CategoryID = item.CategoryID,
                    Title = item.Title,
                    Details = item.Details,
                    PublishDate = item.PublishDate,
                    IsApproved = item.IsApproved,
                    Pic = item.Pic,
                    Category = new CategoryDTO
                    {
                        CategoryID = item.Category.CategoryID,
                        CategoryName = item.Category.CategoryName
                    }
                });
            }

            return View(articleDTO);
        }

        public IActionResult Details(int id)
        {
            var articles = _articleBLL.GetArticleByCategory(id);
			TempData["CategoryID"] = id;
			return View(articles);
        }

        public IActionResult Create()
        {
            ViewData["CategoryID"] = TempData["CategoryID"];
			return View();
			//return Content("Hello ASP.NET Core MVC!");
		}

        [HttpPost]
        public IActionResult Create(ArticleCreateDTO articleCreateDTO, IFormFile Pic)
        {
            //check file upload
            try
            {
				if (Pic != null)
				{
					if (Helper.IsImageFile(Pic.FileName))
					{
						//save file to wwwroot
						var fileName = $"{Guid.NewGuid()}_{Pic.FileName}";
						var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pics", fileName);
                        articleCreateDTO.Pic = fileName;
						using (var fileStream = new FileStream(filePath, FileMode.Create))
						{
							Pic.CopyTo(fileStream);
						}
					}
				}

				_articleBLL.Insert(articleCreateDTO);
                TempData["message"] = @"<div class='alert alert-success'><strong>Success!</strong>Create Data Article Success !</div>";
				return RedirectToAction("Details", "Articles", new { id = articleCreateDTO.CategoryID });
			}
			catch (Exception ex)
            {
                TempData["message"] = $"<div class='alert alert-danger'><strong>Error!</strong>{ex.Message}</div>";
				return View(articleCreateDTO);
			}
		}

        [HttpPost]
        public IActionResult Delete(int id, ArticleDTO articleDTO)
        {
			try
            {
				_articleBLL.Delete(id);
                TempData["message"] = @"<div class='alert alert-success'><strong>Success!</strong>Delete Data Article Success !</div>";
                return RedirectToAction("Details", "Articles", new { id = id });
            }
			catch (Exception ex)
            {
                TempData["message"] = $"<div class='alert alert-danger'><strong>Error!</strong>{ex.Message}</div>";
				return View(articleDTO);
			}
		}

        public IActionResult Edit(int id)
        {
            var article = _articleBLL.GetArticleById(id);
            ViewData["CategoryID"] = TempData["CategoryID"];
            if (article == null)
            {
                TempData["message"] = @"<div class='alert alert-danger'><strong>Error!</strong>Article Not Found !</div>";
                return RedirectToAction("Details", "Articles", new { id = ViewData["CategoryID"] });
            }
            return View(article);
        }
       
    }
}
