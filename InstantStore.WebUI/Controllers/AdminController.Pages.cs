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
using InstantStore.WebUI.Resources;
using InstantStore.Domain.Entities;
using InstantStore.WebUI.ViewModels.Factories;
using InstantStore.Domain.Helpers;
using System.Diagnostics;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        public ActionResult Pages(Guid? treeSelection)
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Pages);
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            this.ViewData["TreeSelection"] = treeSelection;
            return this.View(new PageViewModel());
        }

        public ActionResult PreviewPage(Guid id, bool import = false, Guid? importToCat = null, int o = 0, int c = 200)
        {
            PageViewModel viewModel = null;
            
            if (id == Guid.Empty)
            {
                viewModel = new PageViewModel { 
                    Id = Guid.Empty,
                    Name = StringResource.admin_HomeShort
                };
            }
            else
            {
                var page = this.repository.GetPageById(id);

                this.ViewData["ProductItemsViewModel"] =
                    CategoryViewModelFactory.CreateCategoryViewModel(this.HttpContext.CurrentUser(), page, c, o, ListingViewProductSettings.AdminSettings);

                if (page.IsCategory())
                {
                    var category = repository.GetCategoryById(page.CategoryId.Value);
                    this.ViewData["CategoryProducts"] = new CategoryProductsViewModel(this.repository, page.Id, o, c) { IsTiles = category.ListType == 2 };
                }

                viewModel = new PageViewModel(page, this.repository);
            }

            this.ViewData["IsImportMode"] = import;
            this.ViewData["importToCat"] = importToCat;
            return this.View(viewModel);
        }

        public ActionResult Page(Guid? id, Guid? parentId, string a, string c = null)
        {
            if (!string.IsNullOrEmpty(a) && id != null && id != Guid.Empty)
            {
                if (string.Equals("delete", a, StringComparison.OrdinalIgnoreCase))
                {
                    this.repository.DeletePage(id.Value);
                    return this.RedirectToAction("Pages", new { treeSelection = parentId != null ? parentId.Value : Guid.Empty });
                }
                if (string.Equals("moveup", a, StringComparison.OrdinalIgnoreCase))
                {
                    this.repository.ChangePagePosition(id.Value, false);
                    return this.RedirectToAction("Pages", new { treeSelection = id.Value });
                }
                if (string.Equals("movedown", a, StringComparison.OrdinalIgnoreCase))
                {
                    this.repository.ChangePagePosition(id.Value, true);
                    return this.RedirectToAction("Pages", new { treeSelection = id.Value });
                }
            }
            
            if (id == Guid.Empty)
            {
                return this.RedirectToAction("Settings", new { t = "main" });
            }

            if (c == "p") 
            {
                return this.Product(id, parentId);
            }

            var pageViewModel = id != null ? new PageViewModel(this.repository, id.Value) : new PageViewModel() { ParentCategoryId = parentId ?? Guid.Empty };
            if (pageViewModel.ContentPage != null && pageViewModel.ContentPage.IsCategory())
            {
                return this.Category(id, parentId ?? pageViewModel.ParentCategoryId);
            }
            else
            {
                this.ViewData["SettingsViewModel"] = this.settingsViewModel;
                this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Pages);

                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.View(pageViewModel);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddAttachment(HttpPostedFileBase attachment)
        {
            Binary pageAttachment = attachment.GetFileBinary();
            if (attachment != null)
            {
                var id = this.repository.AddAttachment(new Attachment
                {
                    Content = pageAttachment,
                    ContentLength = attachment.ContentLength,
                    ContentType = attachment.ContentType,
                    Name = attachment.FileName,
                    UploadedAt = DateTime.Now
                });

                return this.Json(new { Id = id });
            }

            return this.HttpNotFound();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Page(PageViewModel pageViewModel)
        {
            Guid? parentId = pageViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : pageViewModel.ParentCategoryId;
            if (parentId != null && pageViewModel.Id != null && pageViewModel.Id != Guid.Empty && ValidateParentId(parentId.Value, pageViewModel.Id))
            {
                this.ModelState.AddModelError("ParentCategoryId", StringResource.admin_CantAddPageToItselfParent);
            }

            var attachmentIds = pageViewModel.Attachments != null && pageViewModel.Attachments.Any() ? pageViewModel.Attachments.Select(x => x.AttachmentId.Value).ToList() : null;

            if (this.ModelState.IsValid)
            {
                var contentPage = pageViewModel.Id != Guid.Empty ? this.repository.GetPageById(pageViewModel.Id) : null;
                if (contentPage == null)
                {
                    contentPage = new ContentPage
                    {
                        Name = pageViewModel.Name,
                        Text = pageViewModel.Text,
                        ParentId = parentId,
                        ShowInMenu = pageViewModel.ShowInMenu,
                        Position = repository.GetPages(parentId, null).Count,
                    };

                    repository.NewPage(contentPage, attachmentIds);
                }
                else
                {
                    contentPage.Name = pageViewModel.Name;
                    contentPage.Text = pageViewModel.Text;
                    contentPage.ParentId = parentId;
                    contentPage.ShowInMenu = pageViewModel.ShowInMenu;

                    this.repository.UpdateContentPage(contentPage, attachmentIds);
                }

                MenuViewModelFactory.ResetCache();

                return this.RedirectToAction("Pages", new { treeSelection = contentPage.Id });
            }
            else
            {
                this.ViewData["SettingsViewModel"] = this.settingsViewModel;
                this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Pages);
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.View(pageViewModel ?? new PageViewModel());
            }
        }

        private bool ValidateParentId(Guid parentId, Guid id)
        {
            var startTime = DateTime.Now;
            var pageTree = new ContentPageTree(id, new RepositoryPageContentProvider(this.repository));
            var endTime = DateTime.Now;
            Trace.TraceInformation("ContentPageTree build time {0} ms.", (endTime - startTime).TotalMilliseconds);
            
            return pageTree.LookupPage(parentId, pageTree.Root) != null;
        }

        public ActionResult Category(Guid? id, Guid? parentId)
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Pages);
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            var categoryViewModel = id != null ? new CategoryViewModel(this.repository, id.Value) : new CategoryViewModel();
            categoryViewModel.Content.ParentCategoryId = parentId ?? Guid.Empty;
            return this.View("Category", categoryViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Category(CategoryViewModel categoryViewModel)
        {
            Guid? parentId = categoryViewModel.Content.ParentCategoryId == Guid.Empty ? (Guid?)null : categoryViewModel.Content.ParentCategoryId;
            if (parentId != null && categoryViewModel.Content.Id != null && categoryViewModel.Content.Id != Guid.Empty && ValidateParentId(parentId.Value, categoryViewModel.Content.Id))
            {
                this.ModelState.AddModelError("ParentCategoryId", StringResource.admin_CantAddPageToItselfParent);
            }

            if (this.ModelState.IsValid)
            {
                Guid pageId = categoryViewModel.Content.Id;

                if (categoryViewModel.Content.Id != Guid.Empty)
                {
                    var contentPage = repository.GetPageById(categoryViewModel.Content.Id);
                    if (contentPage == null || contentPage.CategoryId == null)
                    {
                        throw new InvalidOperationException(string.Format("Model is invalid state. Category for guid {0} has not been found.", categoryViewModel.Content.Id));
                    }

                    contentPage.Name = categoryViewModel.Content.Name;
                    contentPage.Text = categoryViewModel.Content.Text;
                    contentPage.ShowInMenu = categoryViewModel.Content.ShowInMenu;
                    contentPage.ParentId = parentId;
                    repository.UpdateContentPage(contentPage, null);

                    var category = repository.GetCategoryById(contentPage.CategoryId.Value);
                    category.Name = categoryViewModel.Content.Name;
                    category.ListType = categoryViewModel.ListType;
                    category.Description = categoryViewModel.Content.Text;
                    category.ImageId = categoryViewModel.CategoryImage;
                    category.IsImportant = categoryViewModel.IsImportant;
                    repository.UpdateCategory(category);
                }
                else
                {
                    var categoryId = repository.NewCategory(new Category()
                    {
                        Name = categoryViewModel.Content.Name,
                        ListType = categoryViewModel.ListType,
                        Description = categoryViewModel.Content.Text,
                        ImageId = categoryViewModel.CategoryImage,
                    });

                    pageId = repository.NewPage(new ContentPage
                    {
                        Name = categoryViewModel.Content.Name,
                        Text = categoryViewModel.Content.Text,
                        ShowInMenu = categoryViewModel.Content.ShowInMenu,
                        ParentId = parentId,
                        Position = repository.GetPages(parentId, null).Count + 1,
                        CategoryId = categoryId,
                    }, null);
                }

                MenuViewModelFactory.ResetCache();

                return this.RedirectToAction("Pages", new { treeSelection = pageId });
            }
            else
            {
                this.ViewData["SettingsViewModel"] = this.settingsViewModel;
                this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Pages);
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.View(categoryViewModel ?? new CategoryViewModel());
            }
        }

        public ActionResult AttachmentView(Guid id)
        {
            var attachment = this.repository.GetAttachmentById(id);
            if (attachment == null)
            {
                return this.HttpNotFound();
            }

            return this.View("AttachmentEdit", new AttachmentViewModel(attachment));
        }
    }
}