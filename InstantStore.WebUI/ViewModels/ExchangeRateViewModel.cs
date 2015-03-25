using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using InstantStore.WebUI.Resources;
using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;

namespace InstantStore.WebUI.ViewModels
{
    public class ExchangeRateViewModel
    {
        public ExchangeRateViewModel()
        {

        }

        public ExchangeRateViewModel(IRepository repository)
        {
            this.Initialize(repository);
        }

        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorRequired")]
        public Guid FromId { get; set; }

        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorRequired")]
        public Guid ToId { get; set; }

        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorRequired")]
        public double Rate { get; set; }

        public IList<ExchangeRateItem> ExchangeRateItems { get; private set; }

        public IDictionary<Guid, Currency> Currencies { get; private set; }

        public void Initialize(IRepository repository)
        {
            this.Currencies = repository.GetCurrencies().ToDictionary(x => x.Id);
            this.ExchangeRateItems = repository.GetExchangeRates().Select(r =>
                 new ExchangeRateItem
                    {
                        Id = r.Id.ToString(),
                        CurrencyFrom = this.Currencies[r.FromCurrencyId].Text,
                        CurrencyTo = this.Currencies[r.ToCurrencyId].Text,
                        ConversionRate = r.ConversionRate.HasValue ? r.ConversionRate.Value.ToString("0,0.####") : "N/A",
                        ReverseConversionRate = r.ReverseConversionRate.HasValue ? r.ReverseConversionRate.Value.ToString("0,0.####") : "N/A",
                    }
                ).ToList();
        }
    }

    public class ExchangeRateItem
    {
        public string CurrencyFrom { get; set; }
        
        public string CurrencyTo { get; set; }

        public string ConversionRate { get; set; }

        public string ReverseConversionRate { get; set; }

        public string Id { get; set; }
    }
}