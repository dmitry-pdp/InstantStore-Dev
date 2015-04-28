using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using System.ComponentModel.DataAnnotations;
using InstantStore.Domain.Entities;

namespace InstantStore.WebUI.ViewModels
{
    public class ProductViewModel : PageViewModel
    {
        public ProductViewModel()
        {
        }

        public ProductViewModel(IRepository repository)
        {
            this.Images = new List<Guid>();
            this.CreateTemplatesList(repository);
        }

        public ProductViewModel(IRepository repository, Guid id)
            : base(repository.GetPageByProductId(id))
        {
            this.Initialize(repository);
        }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductIsAvailable")]
        public bool IsAvailable { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductPriceCash")]
        public float PriceCash { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductPriceCashless")]
        public float PriceCashless { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductPrice")]
        public Guid CurrencyId { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductDescriptionLabel")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductImagesLabel")]
        public IList<Guid> Images { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductTemplate")]
        public Guid? TemplateId { get; set; }

        public Guid? AttributesId { get; set; }

        public List<SelectListItem> Currencies { get; private set; }

        public List<SelectListItem> TemplatesList { get; private set; }

        public IList<CustomProperty> Attributes { get; set; }

        public void InitializeRootCategory(IRepository repository)
        {
            this.Currencies = repository.GetCurrencies().Select(currency => new SelectListItem() 
            { 
                Text = currency.Text,
                Value = currency.Id.ToString(), 
                Selected = currency.Id == this.CurrencyId
            })
            .ToList();
        }

        private void Initialize(IRepository repository)
        {
            if (this.ContentPage == null || ((ContentType)this.ContentPage.ContentType) != ContentType.Product)
            {
                throw new InvalidOperationException("Incorrect type being initialized.");
            }

            var product = repository.GetProductById(this.ContentPage.ProductId.Value);
            this.IsAvailable = product.IsAvailable;
            this.PriceCash = product.PriceValueCash != null ? (float)product.PriceValueCash : 0.0f;
            this.PriceCashless = product.PriceValueCashless != null ? (float)product.PriceValueCashless : 0.0f;
            this.CurrencyId = product.PriceCurrencyId != null ? product.PriceCurrencyId.Value : Guid.Empty;
            this.Description = product.Description;
            this.Images = repository.GetImagesForProduct(product.Id);
            this.AttributesId = product.CustomAttributesTemplateId;

            var attributesTemplate = product.CustomAttributesTemplateId != null && product.CustomAttributesTemplateId != Guid.Empty 
                ? repository.GetTemplateById(product.CustomAttributesTemplateId.Value) : null;

            this.TemplateId = attributesTemplate != null ? attributesTemplate.PrototypeId : null;

            this.CreateTemplatesList(repository);

            this.Attributes = attributesTemplate != null ? repository.GetPropertiesForTemplate(attributesTemplate.Id).OrderBy(x => x.Name).ToList() : new List<CustomProperty>();
        }

        private void CreateTemplatesList(IRepository repository)
        {
            this.TemplatesList = repository.GetTemplates().Select(t => new SelectListItem()
            {
                Text = t.Name,
                Value = t.Id.ToString(),
                Selected = this.TemplateId != null && t.Id == this.TemplateId
            })
            .ToList();

            this.TemplatesList.Insert(0, new SelectListItem()
            {
                Text = this.TemplateId == null ? StringResource.admin_PageAttributesChoose : StringResource.admin_PageAttributesNone,
                Value = Guid.Empty.ToString(),
                Selected = this.TemplateId == null
            });
        }
    }
}