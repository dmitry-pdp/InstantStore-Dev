using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.HtmlHelpers;
using System.Data.Linq;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        public ActionResult Pages()
        {
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Pages);
            return this.Authorize() ?? this.View(new PageViewModel(this.repository));
        }

        public ActionResult NewPage()
        {
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Pages);
            return this.Authorize() ?? this.View(new PageViewModel(this.repository));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewPage(PageViewModel pageViewModel, HttpPostedFileBase attachment)
        {
            if (this.ModelState.IsValid)
            {
                Binary pageAttachment = attachment.GetFileBinary();
                Guid? parentId = pageViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : pageViewModel.ParentCategoryId;
                repository.NewPage(new ContentPage {
                    Name = pageViewModel.Name,
                    Text = pageViewModel.Text,
                    ParentId = parentId,
                    ContentType = 1,
                    Position = repository.GetPages(parentId).Count + 1,
                    Attachment = pageAttachment,
                    AttachmentType = pageAttachment != null ? attachment.ContentType : null
                });

                return this.RedirectToAction("Pages");
            }

            pageViewModel.InitializeRootCategory(this.repository);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Pages);
            return this.Authorize() ?? this.View(pageViewModel);
        }

        public ActionResult NewCategory(CategoryViewModel categoryViewModel)
        {
            var viewModel = categoryViewModel ?? new CategoryViewModel();
            viewModel.InitializeRootCategory(this.repository);
            return this.Authorize() ?? this.View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewCategory(CategoryViewModel categoryViewModel, HttpPostedFileBase image)
        {
            if (this.ModelState.IsValid)
            {
                var categoryId = repository.NewCategory(new Category()
                {
                    Name = categoryViewModel.Name,
                    ShowInMenu = categoryViewModel.ShowInMenu,
                    ListType = categoryViewModel.ListType,
                    Description = categoryViewModel.Text,
                    Image = image.GetFileBinary()
                });

                Guid? parentId = categoryViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : categoryViewModel.ParentCategoryId;
                repository.NewPage(new ContentPage
                {
                    Name = categoryViewModel.Name,
                    Text = categoryViewModel.Text,
                    ParentId = parentId,
                    ContentType = 2,
                    Position = repository.GetPages(parentId).Count + 1,
                    CategoryId = categoryId,
                });

                return this.RedirectToAction("Pages");
            }

            var viewModel = categoryViewModel ?? new CategoryViewModel();
            viewModel.InitializeRootCategory(this.repository);
            return this.Authorize() ?? this.View(viewModel);

        }

        public ActionResult NewProduct()
        {
            var viewModel = new ProductViewModel();
            viewModel.InitializeRootCategory(this.repository);
            return this.Authorize() ?? this.View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewProduct(ProductViewModel productViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var productId = repository.NewProduct(new Product()
                {
                    Name = productViewModel.Name,
                    Description = productViewModel.Description,
                    IsAvailable = productViewModel.IsAvailable,
                    PriceCurrencyId = productViewModel.CurrencyId,
                    PriceValueCash = new Decimal(productViewModel.PriceCash),
                    PriceValueCashless = new Decimal(productViewModel.PriceCachless)
                });

                Guid? parentId = productViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : productViewModel.ParentCategoryId;
                repository.NewPage(new ContentPage
                {
                    Name = productViewModel.Name,
                    Text = productViewModel.Description,
                    ParentId = parentId,
                    ContentType = 2,
                    Position = repository.GetPages(parentId).Count + 1,
                    ProductId = productId,
                });

                return this.RedirectToAction("Pages");
            }

            var viewModel = productViewModel ?? new ProductViewModel();
            viewModel.InitializeRootCategory(this.repository);
            return this.Authorize() ?? this.View(viewModel);
        }
    }
}