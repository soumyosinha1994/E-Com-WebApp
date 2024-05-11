using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using BulkyWeb.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context):base(context) 
        {
                _context = context;
        }
        //public void Save()
        //{
        //    _context.SaveChanges();
        //}

        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var OrderFromDB=_context.OrderHeaders.FirstOrDefault(x=> x.Id == id);
            if (OrderFromDB != null)
            {
                OrderFromDB.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus)) 
                { 
                OrderFromDB.PaymentStatus=paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var OrderFromDB = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessionId)) 
            { 
            OrderFromDB.SessionId= sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                OrderFromDB.PaymentIntentId = paymentIntentId;
                OrderFromDB.PaymentDate=DateTime.Now;
            }
        }
    }
}
