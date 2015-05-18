using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels.Helpers
{
    public class CurrencyString
    {
        private decimal price;

        private string currencySymbol;

        private NumberFormatInfo numberFormat;

        public CurrencyString(decimal price, string currencySymbol)
        {
            this.price = price;
            this.currencySymbol = currencySymbol;
            this.numberFormat = new CultureInfo("ru-RU").NumberFormat;
            this.numberFormat.CurrencySymbol = string.Empty;
        }

        public override string ToString()
        {
            return string.Concat(this.price.ToString("C", this.numberFormat), " ", this.currencySymbol);
        }
    }
}