using InstantStore.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public IList<ExchangeRate> GetExchangeRates()
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.ExchangeRates.ToList();
            }
        }

        public void AddExchangeRate(ExchangeRate exchangeRate)
        {
            using (var context = new InstantStoreDataContext())
            {
                // check if conversion rate points to the same currency
                if (exchangeRate.FromCurrencyId == exchangeRate.ToCurrencyId)
                {
                    throw new ModelValidationException("Model.ExchangeRate.SameCurrencyError");
                }
                
                // check if such conversion already exists
                if (context.ExchangeRates.Any(r => 
                    (r.FromCurrencyId == exchangeRate.FromCurrencyId && r.ToCurrencyId == exchangeRate.ToCurrencyId) ||
                    (r.ToCurrencyId == exchangeRate.FromCurrencyId && r.FromCurrencyId == exchangeRate.ToCurrencyId)
                ))
                {
                    throw new ModelValidationException("Model.ExchangeRate.AlreadyExists");
                }

                exchangeRate.Id = Guid.NewGuid();
                context.ExchangeRates.InsertOnSubmit(exchangeRate);
                context.SubmitChanges();
            }
        }

        public void DeleteExchangeRate(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                var exchangeRate = context.ExchangeRates.FirstOrDefault(e => e.Id == id);
                if (exchangeRate != null)
                {
                    context.ExchangeRates.DeleteOnSubmit(exchangeRate);
                    context.SubmitChanges();
                }
            }
        }

        public void UpdateExchangeRate(ExchangeRate rate)
        {
            using (var context = new InstantStoreDataContext())
            {
                var exchangeRate = context.ExchangeRates.FirstOrDefault(e => e.Id == rate.Id);
                if (exchangeRate != null)
                {
                    exchangeRate.ConversionRate = rate.ConversionRate;
                    exchangeRate.ReverseConversionRate = rate.ReverseConversionRate;
                    context.SubmitChanges();
                }
            }
        }
    }
}
