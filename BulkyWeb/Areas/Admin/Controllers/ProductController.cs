using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var ProductList = _unitOfWork.Product.GetAll().ToList();
            return View(ProductList);
        }
        public IActionResult Upsert(int? id)
        {
            //EF Projection
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

            //ViewBag.CategoryList = CategoryList;

            //ViewData["CategoryList"] = CategoryList;

            ProductVM productVM = new()
            {
                CategoryList = CategoryList,
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create/Insert
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                //Handle File Upload
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    string productPath= Path.Combine(wwwRootPath, @"images\product");
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                //Create new Product
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();

                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }

        }
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? product = _unitOfWork.Product.Get(u => u.Id == id);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(product);
        //}
        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(product);
        //        _unitOfWork.Save();

        //        TempData["success"] = "Product updated successfully";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    return View();
        //}
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? product = _unitOfWork.Product.Get(u => u.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Product? product = _unitOfWork.Product.Get(u => u.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork?.Save();

            TempData["delete"] = "Product deleted successfully";
            return RedirectToAction("Index", "Product");
        }
    }
}
