using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public void AddUser(User user)
        {
            using (var context = new InstantStoreDataContext())
            {
                user.Id = Guid.NewGuid();
                context.Users.InsertOnSubmit(user);
                context.SubmitChanges();
            }
        }
    }
}
