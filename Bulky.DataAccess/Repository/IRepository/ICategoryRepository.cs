using Bulky.Models;
using Microsoft.EntityFrameworkCore.Update.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository:IRepository<Category>
    {
        public void Update(Category category);
        //public void Save();
    }
}
