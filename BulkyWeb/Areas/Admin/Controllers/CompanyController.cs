using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var Company=_unitOfWork.Company.GetAll().ToList();
            return View(Company);
        }
        public IActionResult Upsert(int? id)
        {
            Company? company = _unitOfWork.Company.Get(u => u.Id == id);

            if (id == null || id == 0)
            {
                company = new();
                return View(company);
            }
            return View(company);
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            string Message = "";
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                    Message = "Company created successfully";
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                    Message = "Company updated successfully";
                }
                _unitOfWork.Save();
                TempData["success"] = Message;
                return RedirectToAction("Index", "Company");
            }
            else
            {
                return View(company);
            }
        }
        #region API CALLS for for Data Tables using JQuery
        [HttpGet]
        public IActionResult GetAll()
        {
            var CompanyList = _unitOfWork.Company.GetAll().ToList().OrderBy(x => x.Id);
            return Json(new { data = CompanyList });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company? company = _unitOfWork.Company.Get(u => u.Id == id);

            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(company);
            _unitOfWork?.Save();
            TempData["delete"] = "Category deleted successfully";
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
