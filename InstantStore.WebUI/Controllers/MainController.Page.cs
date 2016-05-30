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

            this.ViewData["Id"] = id.Value;
            this.ViewData["IsAdmin"] = user != null && user.IsAdmin;

            var contentPage = this.repository.GetPageById(id.Value);
            if (contentPage != null)
            {
                return this.ContentPage(contentPage, id.Value, user, c, o);
            }
            else if ((product = repository.GetProductById(id.Value)) != null)
            {
                this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, parentId);
                this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, parentId, this.Request);

                //this.ViewData["MediaListViewModel"] = CategoryViewModelFactory.CreateSimilarProducts(repository, parentId);

                var productViewModel = new ProductViewModel();
                productViewModel.Initialize(this.repository, id.Value, parentId, user);

                this.AppendTag(productViewModel.Name);
                return this.View("Product", productViewModel);
            }
            else
            {
                return this.HttpNotFound();
            }
        }

        private ActionResult ContentPage(ContentPage page, Guid id, User user, int c, int o)
        {
            // Page/Category view
            this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, id);
            this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, id, this.Request);

            var viewModel = new PageViewModel(page, this.repository);

            var category = CategoryViewModelFactory.CreateCategoryViewModel(user, page, c, o, ListingViewProductSettings.User);
            this.ViewData["ProductItemsViewModel"] = category;
            this.AppendTag(viewModel.Name);

            this.ViewData["IsPage"] = !viewModel.ContentPage.IsCategory();
            return this.View(viewModel);
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