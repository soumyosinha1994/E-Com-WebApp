using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM orderVM { get; set; }
       
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            orderVM= new()
            {
                orderHeader = _unitOfWork.orderHeader.Get(x => x.Id == orderId, IncludeProperty: "ApplicationUser"),
                orderDetail = _unitOfWork.orderDetail.GetAll(x => x.OrderHeaderId == orderId, IncludeProperty: "Product")
            };
            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.orderHeader.Get(u => u.Id == orderVM.orderHeader.Id);
            orderHeaderFromDb.Name = orderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.orderHeader.City;
            orderHeaderFromDb.State = orderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            }
            _unitOfWork.orderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Updated Successfully";

            return RedirectToAction("Details",new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.orderHeader.UpdateStatus(orderVM.orderHeader.Id,SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Processed Successfully";

            return RedirectToAction("Details", new { orderId = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderVM.orderHeader.Id);
            orderHeader.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.orderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped Successfully";

            return RedirectToAction("Details", new { orderId = orderVM.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderVM.orderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.orderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.orderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully";

            return RedirectToAction("Details", new { orderId = orderVM.orderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_Pay_Now()
        {

            orderVM.orderHeader = _unitOfWork.orderHeader.Get(x => x.Id == orderVM.orderHeader.Id, IncludeProperty: "ApplicationUser");
            orderVM.orderDetail = _unitOfWork.orderDetail.GetAll(x => x.OrderHeaderId == orderVM.orderHeader.Id, IncludeProperty: "Product");

            //stripe logic
            var domain = "https://localhost:7280/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/Order/PaymentConfirmation?orderHeaderId={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/Order/details?orderId={orderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in orderVM.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.orderHeader.UpdateStripePaymentID(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.orderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.orderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }

            }

            return View(orderHeaderId);
        }


        #region API CALLS for Data Tables using JQuery

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderList = _unitOfWork.orderHeader.GetAll(IncludeProperty: "ApplicationUser").ToList();

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderList = _unitOfWork.orderHeader.GetAll(IncludeProperty: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderList = _unitOfWork.orderHeader.GetAll(u => u.ApplicationUserId == userId, IncludeProperty: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderList = objOrderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment || u.PaymentStatus == "Pending");
                    break;
                case "inprocess":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = objOrderList });

        }

        #endregion
    }
}
