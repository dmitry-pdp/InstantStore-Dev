namespace InstantStore.WebUI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using InstantStore.Domain.Abstract;
    using InstantStore.Domain.Concrete;
    using InstantStore.Domain.Helpers;
    using InstantStore.WebUI.Resources;
    using System.ComponentModel.DataAnnotations;
    using InstantStore.Domain.Entities;
    using InstantStore.WebUI.ViewModels.Helpers;

    public class ProductViewModel
    {
        public ProductViewModel()
        {
            this.Images = new List<Guid>();
        }

        public Guid Id { get; set; }

        public Guid ParentCategoryId { get; set; }

        public bool IsCloneable { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_Name")]
        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "admin_ProductNameRequiredErrorMessage")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_PageContent")]
        public string Text { get; set; }

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

        public Guid? MainImage { get; set; }

        public List<SelectListItem> Currencies { get; private set; }

        public List<SelectListItem> TemplatesList { get; private set; }

        public IList<CustomProperty> Attributes { get; set; }

        public NavigationLink AddToCart { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_ProductIndex")]
        public int Position { get; set; }

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

        public void Initialize(IRepository repository)
        {
            this.CreateTemplatesList(repository);
        }

        public void Initialize(IRepository repository, Guid? parentId)
        {
            this.Initialize(repository);
            this.ParentCategoryId = parentId ?? Guid.Empty;
            this.Position = -1;
        }

        public void Initialize(IRepository repository, Guid id, Guid? parentId, User user)
        {
            this.Initialize(repository, parentId);
            
            var product = repository.GetProductById(id);

            this.Id = product.VersionId;
            this.Name = product.Name;
            this.Text = product.Description;
            this.IsAvailable = product.IsAvailable;
            this.PriceCash = product.PriceValueCash != null ? (float)product.PriceValueCash : 0.0f;
            this.PriceCashless = product.PriceValueCashless != null ? (float)product.PriceValueCashless : 0.0f;
            this.CurrencyId = product.PriceCurrencyId != null ? product.PriceCurrencyId.Value : Guid.Empty;
            this.Description = product.Description;
            this.Images = repository.GetImagesForProduct(product.Id);
            this.AttributesId = product.CustomAttributesTemplateId;
            this.MainImage = product.MainImageId ?? (this.Images != null ? this.Images.FirstOrDefault() : (Guid?)null);
            this.IsCloneable = true;

            if (parentId != null)
            {
                this.Position = repository.GetProductPosition(id, parentId.Value);
            }

            var attributesTemplate = product.CustomAttributesTemplateId != null && product.CustomAttributesTemplateId != Guid.Empty 
                ? repository.GetTemplateById(product.CustomAttributesTemplateId.Value) : null;

            this.TemplateId = attributesTemplate != null ? attributesTemplate.PrototypeId : null;

            this.Attributes = attributesTemplate != null ? repository.GetPropertiesForTemplate(attributesTemplate.Id).OrderBy(x => x.Name).ToList() : new List<CustomProperty>();
      
            if (user != null)
            {
                if (!user.IsAdmin && user.DefaultCurrencyId != null)
                {
                    var userCurrency = repository.GetCurrencies().FirstOrDefault(x => x.Id == user.DefaultCurrencyId.Value);
                    var price = product.GetPriceForUser(user, repository.GetExchangeRates());

                    this.Attributes.Add(new CustomProperty
                    {
                        Name = StringResource.Price,
                        Value = userCurrency != null ? new CurrencyString(price, userCurrency.Text).ToString() : null
                    });
                    /*
                    this.AddToCart = new NavigationLink
                    {
                        ControllerName = "User",
                        ActionName = "AddToCart",
                        PageId = this.Id,
                        Text = StringResource.productTile_AddToCart
                    };
                    */
                }
                else if (user.IsAdmin && product.PriceCurrencyId != null && product.PriceValueCash != null)
                {
                    var currency = repository.GetCurrencies().FirstOrDefault(x => x.Id == product.PriceCurrencyId.Value);

                    this.Attributes.Add(new CustomProperty
                    {
                        Name = StringResource.Price,
                        Value = currency != null ? new CurrencyString(product.PriceValueCash.Value, currency.Text).ToString() : null
                    });
                }
            }
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