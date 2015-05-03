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

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        public ActionResult Pages(Guid? treeSelection)
        {
            this.ViewData["TreeSelection"] = treeSelection;
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            return this.Authorize() ?? this.View(new PageViewModel());
        }

        public ActionResult PreviewPage(Guid id, bool import = false, Guid? importToCat = null, int p = 1, int c = 15)
        {
            var page = this.repository.GetPageById(id);
            if ((ContentType)page.ContentType == ContentType.Category && page.CategoryId != null)
            {
                var category = repository.GetCategoryById(page.CategoryId.Value);
                this.ViewData["CategoryProducts"] = new CategoryProductsViewModel(this.repository, page.Id, p, c) { IsTiles = category.ListType == 2 };
            }

            this.ViewData["IsImportMode"] = import;
            this.ViewData["importToCat"] = importToCat;
            return this.View(new PageViewModel(page, false));
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

            if (c == "p") 
            {
                return this.Product(id, null);
            }

            var pageViewModel = id != null ? new PageViewModel(this.repository, id.Value) : new PageViewModel() { ParentCategoryId = parentId ?? Guid.Empty };
            if (pageViewModel.ContentPage.ContentType == (int)(ContentType.Category))
            {
                return this.Category(id, parentId);
            }
            else
            {
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.Authorize() ?? this.View(pageViewModel);
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
        public ActionResult Page(PageViewModel pageViewModel, Guid? attachmentId)
        {
            if (this.ModelState.IsValid)
            {
                Guid? parentId = pageViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : pageViewModel.ParentCategoryId;
                var contentPage = pageViewModel.Id != Guid.Empty ? this.repository.GetPageById(pageViewModel.Id) : null;
                if (contentPage == null)
                {
                    repository.NewPage(new ContentPage
                    {
                        Name = pageViewModel.Name,
                        Text = pageViewModel.Text,
                        ParentId = parentId,
                        ContentType = (int)ContentType.Page,
                        Position = repository.GetPages(parentId, null).Count,
                        AttachmentId = attachmentId,
                    });
                }
                else
                {
                    contentPage.Name = pageViewModel.Name;
                    contentPage.Text = pageViewModel.Text;
                    contentPage.ParentId = parentId;
                    contentPage.AttachmentId = attachmentId;

                    this.repository.UpdateContentPage(contentPage);
                }

                return this.RedirectToAction("Pages");
            }
            else
            {
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.Authorize() ?? this.View(pageViewModel ?? new PageViewModel());
            }
        }

        public ActionResult Category(Guid? id, Guid? parentId)
        {
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            var categoryViewModel = id != null ? new CategoryViewModel(this.repository, id.Value) : new CategoryViewModel();
            categoryViewModel.Content.ParentCategoryId = parentId ?? Guid.Empty;
            return this.Authorize() ?? this.View("Category", categoryViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Category(CategoryViewModel categoryViewModel)
        {
            if (this.ModelState.IsValid)
            {
                Guid? parentId = categoryViewModel.Content.ParentCategoryId == Guid.Empty ? (Guid?)null : categoryViewModel.Content.ParentCategoryId;

                if (categoryViewModel.Content.Id != Guid.Empty)
                {
                    var contentPage = repository.GetPageById(categoryViewModel.Content.Id);
                    if (contentPage == null || contentPage.CategoryId == null)
                    {
                        throw new InvalidOperationException(string.Format("Model is invalid state. Category for guid {0} has not been found.", categoryViewModel.Content.Id));
                    }

                    contentPage.Name = categoryViewModel.Content.Name;
                    contentPage.Text = categoryViewModel.Content.Text;
                    contentPage.ParentId = parentId;
                    repository.UpdateContentPage(contentPage);

                    var category = repository.GetCategoryById(contentPage.CategoryId.Value);
                    category.Name = categoryViewModel.Content.Name;
                    category.ShowInMenu = categoryViewModel.ShowInMenu;
                    category.ListType = categoryViewModel.ListType;
                    category.Description = categoryViewModel.Content.Text;
                    category.ImageId = categoryViewModel.CategoryImage;
                    repository.UpdateCategory(category);
                }
                else
                {
                    var categoryId = repository.NewCategory(new Category()
                    {
                        Name = categoryViewModel.Content.Name,
                        ShowInMenu = categoryViewModel.ShowInMenu,
                        ListType = categoryViewModel.ListType,
                        Description = categoryViewModel.Content.Text,
                        ImageId = categoryViewModel.CategoryImage,
                    });

                    repository.NewPage(new ContentPage
                    {
                        Name = categoryViewModel.Content.Name,
                        Text = categoryViewModel.Content.Text,
                        ParentId = parentId,
                        ContentType = (int)ContentType.Category,
                        Position = repository.GetPages(parentId, null).Count + 1,
                        CategoryId = categoryId,
                    });
                }

                return this.RedirectToAction("Pages");
            }
            else
            {
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.Authorize() ?? this.View(categoryViewModel ?? new CategoryViewModel());
            }
        }

        public ActionResult ContentSummary(Guid? id)
        {
            if (id == null)
            {
                return this.HttpNotFound();
            }

            return this.View(new ContentSummaryViewModel(repository, id.Value));
        }

        public ActionResult AttachmentView(Guid id)
        {
            var attachment = this.repository.GetAttachmentById(id);
            if (attachment == null)
            {
                return this.HttpNotFound();
            }

            return this.View("Attachment", new AttachmentViewModel(attachment));
        }
    }
}