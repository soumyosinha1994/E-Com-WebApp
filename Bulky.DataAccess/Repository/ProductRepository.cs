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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context):base(context) 
        {
                _context = context;
        }
        //public void Save()
        //{
        //    _context.SaveChanges();
        //}

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }
    }
}
