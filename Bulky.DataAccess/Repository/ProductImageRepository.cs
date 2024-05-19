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
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private ApplicationDbContext _context;
        public ProductImageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(ProductImage obj)
        {
            _context.productImages.Update(obj);
        }
    }
}
