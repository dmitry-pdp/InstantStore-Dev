using InstantStore.WebUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    /// <summary>
    /// Static map to return localized string for the error code.
    /// </summary>
    public static class ErrorCodeMap
    {
        private static Dictionary<string, Func<string>> localizationMap = new Dictionary<string, Func<string>>
        {
            { "Model.ExchangeRate.SameCurrencyError", () => StringResource.error_ExchangeRateSameCurrency },
            { "Model.ExchangeRate.AlreadyExists", () => StringResource.error_ExchangeRateAlreadyExists }
        };

        /// <summary>
        /// Gets the localized string for the error.
        /// </summary>
        /// <param name="errorCode">Error code with namespace.</param>
        /// <returns></returns>
        public static string GetStringForError(string errorCode)
        {
            Func<string> func;
            return localizationMap.TryGetValue(errorCode, out func) && func != null ? func() : null;
        }
    }
}