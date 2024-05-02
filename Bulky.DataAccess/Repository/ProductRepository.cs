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
            var objfromDb=_context.Products.FirstOrDefault(x => x.Id == product.Id);
            if (objfromDb != null) 
            {
                objfromDb.Title = product.Title;
                objfromDb.ISBN = product.ISBN;
                objfromDb.Price = product.Price;
                objfromDb.Price50 = product.Price50;
                objfromDb.ListPrice = product.ListPrice;
                objfromDb.Price100 = product.Price100;
                objfromDb.Description = product.Description;
                objfromDb.CategoryId = product.CategoryId;
                objfromDb.Author = product.Author;
                if (product.ImageUrl != null)
                {
                    objfromDb.ImageUrl = product.ImageUrl;
                }

            }
        }
    }
}
