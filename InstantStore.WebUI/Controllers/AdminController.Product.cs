using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Entities;
using InstantStore.WebUI.HtmlHelpers;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        public ActionResult Product(Guid? id, Guid? parentId)
        {
            var viewModel = id != null && id != Guid.Empty 
                ? new ProductViewModel(this.repository, id.Value, parentId, null) 
                : new ProductViewModel(this.repository, parentId);

            viewModel.InitializeRootCategory(this.repository);
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Offers);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            return this.View("Product", viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Product(ProductViewModel productViewModel)
        {
            if (productViewModel == null)
            {
                return this.HttpNotFound();
            }

            if (this.ModelState.IsValid)
            {
                repository.UpdateOrCreateNewProduct(new Product()
                {
                    Id = productViewModel.Id,
                    Name = productViewModel.Name,
                    Description = productViewModel.Description,
                    IsAvailable = productViewModel.IsAvailable,
                    PriceCurrencyId = productViewModel.CurrencyId,
                    PriceValueCash = new Decimal(productViewModel.PriceCash),
                    PriceValueCashless = new Decimal(productViewModel.PriceCashless)
                },
                    productViewModel.ParentCategoryId,
                    productViewModel.Images,
                    productViewModel.TemplateId,
                    productViewModel.Attributes);

                return this.RedirectToAction("Pages", new { treeSelection = productViewModel.ParentCategoryId });
            }
            else
            {
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.View(productViewModel);
            }
        }

        public ActionResult ProductImage(Guid imageId)
        {
            this.ViewData["ImageId"] = imageId.ToString();
            return this.View("ProductImage", imageId);
        }

        public ActionResult EditProductsPartial(Guid id, int p = 0, int c = 15)
        {
            return this.View("EditProductDetails", new CategoryProductsViewModel(this.repository, id, p, c));
        }

        public ActionResult ProductAttributes(Guid id, Guid tid)
        {
            if (id == Guid.Empty)
            {
                return this.HttpNotFound();
            }

            if (tid == Guid.Empty)
            {
                return new EmptyResult();
            }

            var template = repository.GetTemplateById(tid);

            return this.View(new TemplateViewModel(repository.CreateAttributesForProduct(id, tid).OrderBy(x => x.Name)) { Name = template.Name });
        }

        public ActionResult ImportProducts(Guid parentId)
        {
            this.ViewData["ParentCategory"] = parentId;
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            return this.View();
        }

        [HttpPost]
        public ActionResult ImportProducts(CategoryProductsViewModel data, Guid importPageId)
        {
            if (importPageId == data.ParentCategoryId)
            {
                return this.Json(new { status = "Error", message = StringResource.admin_ImportProductErrorSameCategory });
            }

            if (data.Products == null || data.Products.Count(x => x.Checked) == 0)
            {
                return this.Json(new { status = "Error", message = StringResource.admin_ImportProductErrorNoItemChecked });
            }

            this.repository.AssignProductsToCategory(data.Products.Where(x => x.Checked).Select(x => x.Id).ToList(), importPageId);

            return this.Json(new { status = "success" });
        }
    }
}