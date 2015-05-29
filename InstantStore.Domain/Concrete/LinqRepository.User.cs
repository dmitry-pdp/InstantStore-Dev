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
                user.DefaultCurrencyId = user.DefaultCurrencyId == null ? context.Currencies.FirstOrDefault().Id : user.DefaultCurrencyId;
                user.Id = Guid.NewGuid();
                user.Password = PasswordHash.PasswordHash.CreateHash(user.Password);
                context.Users.InsertOnSubmit(user);
                context.SubmitChanges();
            }
        }

        public User Login(string userName, string password)
        {
            using (var context = new InstantStoreDataContext())
            {
                var query = userName.Contains('@') 
                    ? (Func<User, bool>)((User u) => string.Equals(u.Email, userName, StringComparison.OrdinalIgnoreCase))
                    : (Func<User, bool>)((User u) => string.Equals(u.Name, userName, StringComparison.OrdinalIgnoreCase));

                var user = context.Users.FirstOrDefault(query);
                if (user == null)
                {
                    return null;
                }

                return PasswordHash.PasswordHash.ValidatePassword(password, user.Password) ? user : null;
            }
        }

        public User GetUser(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Users.FirstOrDefault(user => user.Id == id);
            }
        }

        public IList<User> GetUsers(Func<User, bool> condition)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Users.Where(condition).ToList();
            }
        }

        public void ActivateUser(Guid userId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                user.IsActivated = true;
                context.SubmitChanges();
            }
        }

        public void UnblockUser(Guid userId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                user.IsBlocked = false;
                context.SubmitChanges();
            }
        }

        public void BlockUser(Guid userId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var user = context.Users.First(u => u.Id == userId);
                user.IsBlocked = true;
                context.SubmitChanges();
            }
        }

        public void UpdateUser(User user)
        {
            using (var context = new InstantStoreDataContext())
            {
                var userUpdated = context.Users.FirstOrDefault(u => u.Id == user.Id);
                userUpdated.Name = user.Name;
                userUpdated.City = user.City;
                userUpdated.Company = user.Company;
                userUpdated.DefaultCurrencyId = user.DefaultCurrencyId != Guid.Empty ? user.DefaultCurrencyId : null;
                userUpdated.Email = user.Email;
                userUpdated.IsPaymentCash = user.IsPaymentCash;
                userUpdated.Phonenumber = user.Phonenumber;
                userUpdated.Comments = user.Comments;
                context.SubmitChanges();
            }
        }

        public void ResetPassword(Guid userId, string newPassword)
        {
            using (var context = new InstantStoreDataContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                user.Password = PasswordHash.PasswordHash.CreateHash(newPassword);
                context.SubmitChanges();
            }
        }
    }
}
