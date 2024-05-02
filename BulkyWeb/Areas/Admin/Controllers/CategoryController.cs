using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        //private readonly ApplicationDbContext _applicationDbContext;
        //private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            //var CategoryList = _applicationDbContext.Categories.ToList();
            //var CategoryList = _categoryRepository.GetAll().ToList();
            var CategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(CategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name.");
            }
            //if (category?.Name?.ToLower() == "test")
            //{
            //    ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name.");
            //}
            if (ModelState.IsValid)
            {
                //_applicationDbContext.Categories.Add(category);
                //_applicationDbContext.SaveChanges();

                //_categoryRepository.Add(category);
                //_categoryRepository.Save();

                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();

                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index", "Category");
            }
            return View();

        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Category category1 = _applicationDbContext.Categories.FirstOrDefault(c => c.Id == id);
            //Category? category1 = _applicationDbContext.Categories.Where(c => c.Id == id).FirstOrDefault();
            //Category? category = _applicationDbContext?.Categories?.Find(id);

            //Category? category = _categoryRepository.Get(u => u.Id == id);

            Category? category = _unitOfWork.Category.Get(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {

            if (ModelState.IsValid)
            {
                //_applicationDbContext.Categories.Update(category);
                //_applicationDbContext.SaveChanges();

                //_categoryRepository.Update(category);
                //_categoryRepository.Save();

                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();

                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index", "Category");
            }
            return View();

        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Category category1 = _applicationDbContext.Categories.FirstOrDefault(c => c.Id == id);
            //Category? category1 = _applicationDbContext.Categories.Where(c => c.Id == id).FirstOrDefault();
            //Category? category = _applicationDbContext?.Categories?.Find(id);

            //Category? category = _categoryRepository.Get(u => u.Id == id);

            Category? category = _unitOfWork.Category.Get(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            //Category? category = _applicationDbContext?.Categories?.Find(id);
            //Category? category = _categoryRepository.Get(u => u.Id == id);

            Category? category = _unitOfWork.Category.Get(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
            }
            //_applicationDbContext?.Categories.Remove(category);
            //_applicationDbContext?.SaveChanges();

            //_categoryRepository?.Remove(category);
            //_categoryRepository?.Save();

            _unitOfWork.Category.Remove(category);
            _unitOfWork?.Save();

            TempData["delete"] = "Category deleted successfully";
            return RedirectToAction("Index", "Category");
        }
    }
}
