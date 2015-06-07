using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Helpers;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    public partial class MainController
    {
        public ActionResult Page(Guid? id, Guid? parentId, int c = 200, int o = 0)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.RedirectToAction("Index");
            }

            this.Initialize(id.Value);

            Product product;

            var user = UserIdentityManager.GetActiveUser(this.Request, repository);

            var contentPage = this.repository.GetPageById(id.Value);
            if (contentPage != null)
            {
                // Page/Category view
                this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, id);
                this.ViewData["NavigationMenuViewModel"] = 
                    MenuViewModelFactory.CreateNavigationMenu(repository, id);

                var viewModel = new PageViewModel(contentPage);
                if (viewModel.Attachment != null)
                {
                    viewModel.Attachment.CanEdit = false;
                }

                this.ViewData["ProductItemsViewModel"] = CategoryViewModelFactory.CreateCategoryViewModel(user, contentPage, c, o, user != null ? ListingViewProductSettings.User : ListingViewProductSettings.User);

                return this.View(viewModel);
            }
            else if ((product = repository.GetProductById(id.Value)) != null)
            {
                if (parentId != null)
                {
                    this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, parentId);
                }

                this.ViewData["NavigationMenuViewModel"] =
                    MenuViewModelFactory.CreateNavigationMenu(repository, parentId);

                this.ViewData["MediaListViewModel"] = CategoryViewModelFactory.CreateSimilarProducts(repository, parentId);

                var productViewModel = new ProductViewModel(this.repository, id.Value, parentId, user);
                return this.View("Product", productViewModel);
            }
            else
            {
                return this.HttpNotFound();
            }
        }

        public ActionResult Download(Guid id)
        {
            var attachment = this.repository.GetAttachmentById(id);
            if (attachment == null)
            {
                return this.HttpNotFound();
            }

            var stream = new MemoryStream(attachment.Content.ToArray());
            return new FileStreamResult(stream, attachment.ContentType);
        }
    }
}