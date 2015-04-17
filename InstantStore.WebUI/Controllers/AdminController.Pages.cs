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

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        public ActionResult Pages(Guid? treeSelection)
        {
            this.ViewData["TreeSelection"] = treeSelection;
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Pages);
            return this.Authorize() ?? this.View(new PageViewModel());
        }

        public ActionResult Page(Guid? id, Guid? parentId, string a)
        {
            if (!string.IsNullOrEmpty(a) && id != null && id != Guid.Empty)
            {
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

            var pageViewModel = id != null ? new PageViewModel(this.repository, id.Value) : new PageViewModel() { ParentCategoryId = parentId ?? Guid.Empty };
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            return this.Authorize() ?? this.View(pageViewModel);
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
                var contentPage = this.repository.GetPageById(pageViewModel.Id);
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

        public ActionResult Category(Guid? id)
        {
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            var categoryViewModel = id != null ? new CategoryViewModel(this.repository, id.Value) : new CategoryViewModel();
            return this.Authorize() ?? this.View(categoryViewModel);
        }

        public ActionResult NewProduct(Guid? guid)
        {
            var viewModel = new ProductViewModel();
            viewModel.InitializeRootCategory(this.repository);
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            return this.Authorize() ?? this.View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Category(CategoryViewModel categoryViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var categoryId = repository.NewCategory(new Category()
                {
                    Name = categoryViewModel.Name,
                    ShowInMenu = categoryViewModel.ShowInMenu,
                    ListType = categoryViewModel.ListType,
                    Description = categoryViewModel.Text,
                    ImageId = categoryViewModel.CategoryImage
                });

                Guid? parentId = categoryViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : categoryViewModel.ParentCategoryId;
                repository.NewPage(new ContentPage
                {
                    Name = categoryViewModel.Name,
                    Text = categoryViewModel.Text,
                    ParentId = parentId,
                    ContentType = (int)ContentType.Category,
                    Position = repository.GetPages(parentId, null).Count + 1,
                    CategoryId = categoryId,
                });

                return this.RedirectToAction("Pages");
            }
            else
            {
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.Authorize() ?? this.View(categoryViewModel ?? new CategoryViewModel());
            }
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
                    ContentType = (int)ContentType.Product,
                    Position = repository.GetPages(parentId, null).Count + 1,
                    ProductId = productId,
                });

                return this.RedirectToAction("Pages");
            }
            else
            {
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.Authorize() ?? this.View(productViewModel ?? new ProductViewModel());
            }
        }

        public ActionResult ProductImage(Guid imageId)
        {
            this.ViewData["ImageId"] = imageId.ToString();
            return this.View("ProductImage");
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