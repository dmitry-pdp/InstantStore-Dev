using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using InstantStore.Domain.Exceptions;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        [HttpGet]
        public ActionResult Currency(string tab, ExchangeRateViewModel exchangeRateViewModel)
        {
            var currencies = this.repository.GetCurrencies();
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Currency);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Currency);
            this.ViewData["Currencies"] = currencies;
            this.ViewData["Tab"] = tab;
            this.ViewData["CurrenciesSelectList"] = 
                currencies.Select(c => new SelectListItem {
                    Text = c.Text,
                    Value = c.Id.ToString()
                }).ToList();

            if (exchangeRateViewModel == null)
            {
                exchangeRateViewModel = new ExchangeRateViewModel(this.repository);
            }
            else
            {
                exchangeRateViewModel.Initialize(this.repository);
            }

            return this.View("Currency", exchangeRateViewModel);
        }

        [HttpGet]
        public ActionResult CurrencyUpdate(string a, Guid? id)
        {
            if (string.Equals(a, "delete") && id != null)
            {
                this.repository.DeleteCurrency(id.Value);
            }

            return this.RedirectToAction("Currency", new { tab = "currency" });
        }

        [HttpPost]
        public ActionResult CurrencyUpdate(Currency currency)
        {
            if (!string.IsNullOrWhiteSpace(currency.Text))
            {
                this.repository.AddCurrency(currency.Text);
            }

            return this.RedirectToAction("Currency", new { tab = "currency" });
        }

        [HttpGet]
        public ActionResult ExchangeRateUpdate(Guid? id, string a)
        {
            if (string.Equals(a, "delete") && id != null)
            {
                this.repository.DeleteExchangeRate(id.Value);
            }

            return this.RedirectToAction("Currency");
        }

        [HttpPost]
        public ActionResult ExchangeRateUpdate(string action, string rate, string rate_r, Guid? id, ExchangeRateViewModel newRate)
        {
            // ajax update
            if (string.Equals(action, "update", StringComparison.OrdinalIgnoreCase) &&  id != null)
            {
                this.repository.UpdateExchangeRate(new ExchangeRate
                {
                    Id = id.Value,
                    ConversionRate = this.TryConvertDouble(rate),
                    ReverseConversionRate = this.TryConvertDouble(rate_r)
                });

                return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
            }

            // new exchange rate post
            if (newRate != null && this.ModelState.IsValid)
            {
                try
                {
                    this.repository.AddExchangeRate(new ExchangeRate
                    {
                        FromCurrencyId = newRate.FromId,
                        ToCurrencyId = newRate.ToId,
                        ConversionRate = newRate.Rate,
                        ReverseConversionRate = newRate.ReverseRate
                    });
                }
                catch(ModelValidationException e)
                {
                    this.ModelState.AddModelError(string.Empty, ErrorCodeMap.GetStringForError(e.Message));
                }
            }

            return this.Currency(null, newRate);
        }

        private double? TryConvertDouble(string input)
        {
            double result;
            return double.TryParse(input, out result) ? (double?)result : (double?)null;
        }
    }
}