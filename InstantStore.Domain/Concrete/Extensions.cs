using InstantStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public static class Extensions
    {
        public static bool IsCategory(this ContentPage page)
        { 
            return page.CategoryId != null;
        }
    }
}
