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

        public User Login(string userName, string password)
        {
            using (var context = new InstantStoreDataContext())
            {
                var query = userName.Contains('@') 
                    ? (Func<User, bool>)((User user) => string.Equals(user.Email, userName, StringComparison.OrdinalIgnoreCase))
                    : (Func<User, bool>)((User user) => string.Equals(user.Name, userName, StringComparison.OrdinalIgnoreCase));
                
                return context.Users.FirstOrDefault(query);
            }
        }

        public User GetUser(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Users.FirstOrDefault(user => user.Id == id);
            }
        }
    }
}
