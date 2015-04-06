using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using System.ComponentModel.DataAnnotations;

namespace InstantStore.WebUI.ViewModels
{
    public class ProductViewModel : PageViewModel
    {
        public ProductViewModel()
        {
            this.Images = new List<Guid>();
        }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductIsAvailable")]
        public bool IsAvailable { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductPriceCash")]
        public float PriceCash { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductPriceCashless")]
        public float PriceCachless { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductPrice")]
        public Guid CurrencyId { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductDescriptionLabel")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductImagesLabel")]
        public List<Guid> Images { get; set; }

        public List<SelectListItem> Currencies { get; private set; }

        public override void InitializeRootCategory(IRepository repository)
        {
            base.InitializeRootCategory(repository);
            this.Currencies = repository.GetCurrencies().Select(currency => new SelectListItem() 
            { 
                Text = currency.Text,
                Value = currency.Id.ToString(), 
                Selected = currency.Id == this.CurrencyId
            })
            .ToList();
        }
    }
}