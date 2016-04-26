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
            var viewModel = new ProductViewModel();

            if (id != null && id != Guid.Empty)
            {
                viewModel.Initialize(repository, id.Value, parentId, null);
            }
            else
            {
                viewModel.Initialize(repository, parentId);
            }

            viewModel.InitializeRootCategory(this.repository);
            
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Pages);
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
                return this.Product(null, null);
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
                    productViewModel.Attributes,
                    productViewModel.Position);

                return this.RedirectToAction("Pages", new { treeSelection = productViewModel.ParentCategoryId });
            }
            else
            {
                productViewModel.Initialize(repository);

                productViewModel.InitializeRootCategory(this.repository);
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Pages);
                this.ViewData["SettingsViewModel"] = this.settingsViewModel;
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

        public ActionResult ProductAttributes(Guid tid)
        {
            if (tid == Guid.Empty)
            {
                return new EmptyResult();
            }

            var template = repository.GetTemplateById(tid);

            return this.View(new TemplateViewModel(repository.CreateAttributesForProduct(tid).OrderBy(x => x.Name)) { Name = template.Name });
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

        public ActionResult DeleteProducts(Guid parentId, int o = 0, int c = 200)
        {
            if (parentId == Guid.Empty)
            {
                return null;
            }

            using (var context = new InstantStoreDataContext())
            {
                return this.View(new CategoryProductsViewModel
                {
                    ParentCategoryId = parentId,
                    Products = context
                        .ProductToCategories
                        .Where(x => x.CategoryId == parentId && x.GroupId == null)
                        .Skip(o)
                        .Take(c)
                        .ToList()
                        .Select(x => 
                        {
                            var product = x.Product;
                            return new CategoryProductViewModel
                            {
                                Id = product.VersionId,
                                Name = product.Name
                            };
                        })
                        .ToList(),
                    IsTiles = false
                });
            }
        }

        [HttpPost]
        public ActionResult DeleteProducts(CategoryProductsViewModel data)
        {
            if (data.Products == null || data.Products.Count(x => x.Checked) == 0)
            {
                return this.Json(new { status = "Error", message = StringResource.admin_ImportProductErrorNoItemChecked });
            }

            this.repository.RemoveProductFromCategory(data.ParentCategoryId, data.Products.Where(x => x.Checked).Select(x => x.Id).ToList());

            return this.Json(new { status = "success" });
        }

        public ActionResult ProductClone(Guid pageId, Guid parentId)
        {
            if (pageId == Guid.Empty || parentId == Guid.Empty)
            {
                return this.RedirectToAction("Information", new { id = pageId });
            }

            var newProductId = this.repository.CloneProduct(pageId, parentId);
            return this.RedirectToAction("Product", new { id = newProductId, parentId = parentId });
        }
    }
}