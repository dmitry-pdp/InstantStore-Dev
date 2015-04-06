using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public Image GetImageById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Images.FirstOrDefault(x => x.Id == id);
            }
        }
    }
}
