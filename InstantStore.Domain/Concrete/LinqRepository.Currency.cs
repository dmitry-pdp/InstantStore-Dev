using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public IList<Currency> GetCurrencies()
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Currencies.ToList();
            }
        }

        public void AddCurrency(string text)
        {
            using (var context = new InstantStoreDataContext())
            {
                var currency = new Currency
                {
                    Id = Guid.NewGuid(),
                    Text = text
                };

                context.Currencies.InsertOnSubmit(currency);
                context.SubmitChanges();
            }
        }

        public void DeleteCurrency(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                var currency = context.Currencies.FirstOrDefault(c => c.Id == id);
                if (currency != null)
                {
                    context.Currencies.DeleteOnSubmit(currency);
                    context.SubmitChanges();
                }
            }
        }
    }
}
