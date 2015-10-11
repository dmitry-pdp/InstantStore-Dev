using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InstantStore.Domain.Concrete;
using InstantStore.Domain.Exceptions;

namespace InstantStore.Domain.Helpers
{
    public static class PriceHelper
    {
        public static decimal GetPriceForUser(this Product product, User user, IEnumerable<ExchangeRate> exchangeRates)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (exchangeRates == null)
            {
                throw new ArgumentNullException("exchangeRates");
            }

            bool sameCurrency = 
                user.IsAdmin ||
                (product.PriceCurrencyId != null && 
                user.DefaultCurrencyId != null && 
                product.PriceCurrencyId == user.DefaultCurrencyId);

            double conversionRate = 1.0;

            if (!sameCurrency)
            {
                var exchangeRate = exchangeRates.Any() && product.PriceCurrencyId != null
                    ? exchangeRates.FirstOrDefault(x => x.FromCurrencyId == product.PriceCurrencyId && x.ToCurrencyId == user.DefaultCurrencyId)
                    : null;

                if (exchangeRate != null && exchangeRate.ConversionRate != null)
                {
                    conversionRate = exchangeRate.ConversionRate.Value;
                }
                else 
                {
                    exchangeRate = exchangeRates.Any() && product.PriceCurrencyId != null
                    ? exchangeRates.FirstOrDefault(x => x.ToCurrencyId == product.PriceCurrencyId && x.FromCurrencyId == user.DefaultCurrencyId)
                    : null;

                    if (exchangeRate != null && exchangeRate.ReverseConversionRate != null)
                    {
                        conversionRate = exchangeRate.ReverseConversionRate.Value;
                    }
                    else
                    {
                        throw new ModelValidationException("InconsistentModel");
                    }
                }

                conversionRate = exchangeRate.ConversionRate.Value;
            }

            decimal productPrice = user.IsPaymentCash ? product.PriceValueCash ?? (decimal)0.0 : product.PriceValueCashless ?? (decimal)0.0;
            decimal priceInUserCurrency = productPrice * (decimal)conversionRate;
            return priceInUserCurrency;
        }
    }
}
