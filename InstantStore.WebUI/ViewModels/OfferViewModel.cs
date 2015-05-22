using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels
{
    public class OfferViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "offer_Error_NameRequired")]
        [StringLength(250, MinimumLength = 3, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "offer_ErrorNameLength")]
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public bool IsMultiApply { get; set; }

        public Guid CurrencyId { get; set; }

        public OfferDiscountType Type { get; set; }

        public int Priority { get; set; }

        [Range(1, double.MaxValue, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "offer_ErrorThresholdRange")]
        public decimal ThresholdPrice { get; set; }

        [Range(1, double.MaxValue, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "offer_ErrorDiscountRange")]
        public decimal Discount { get; set; }

        public bool IsSelected { get; set; }

        public string CurrencyText { get; set; }
    }
}