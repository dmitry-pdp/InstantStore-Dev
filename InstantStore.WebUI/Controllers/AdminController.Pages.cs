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

        public ActionResult PartialPage(Guid id)
        {
            var page = this.repository.GetPageById(id);
            if ((ContentType)page.ContentType == ContentType.Category && page.CategoryId != null)
            {
                this.ViewData["CategoryProducts"] = new CategoryProductsViewModel(this.repository, page.Id);
            }
            return this.View(new PageViewModel(page, false));
        }

        public ActionResult Page(Guid? id, Guid? parentId, string a)
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

        public ActionResult Category(Guid? id)
        {
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            var categoryViewModel = id != null ? new CategoryViewModel(this.repository, id.Value) : new CategoryViewModel();
            return this.Authorize() ?? this.View(categoryViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Category(CategoryViewModel categoryViewModel)
        {
            if (this.ModelState.IsValid)
            {
                Guid? parentId = categoryViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : categoryViewModel.ParentCategoryId;

                if (categoryViewModel.Id != Guid.Empty)
                {
                    var contentPage = repository.GetPageById(categoryViewModel.Id);
                    if (contentPage == null || contentPage.CategoryId == null)
                    {
                        throw new InvalidOperationException(string.Format("Model is invalid state. Category for guid {0} has not been found.", categoryViewModel.Id));
                    }

                    contentPage.Name = categoryViewModel.Name;
                    contentPage.Text = categoryViewModel.Text;
                    contentPage.ParentId = parentId;
                    repository.UpdateContentPage(contentPage);

                    var category = repository.GetCategoryById(contentPage.CategoryId.Value);
                    category.Name = categoryViewModel.Name;
                    category.ShowInMenu = categoryViewModel.ShowInMenu;
                    category.ListType = categoryViewModel.ListType;
                    category.Description = categoryViewModel.Text;
                    category.ImageId = categoryViewModel.CategoryImage;
                    repository.UpdateCategory(category);
                }
                else
                {
                    var categoryId = repository.NewCategory(new Category()
                    {
                        Name = categoryViewModel.Name,
                        ShowInMenu = categoryViewModel.ShowInMenu,
                        ListType = categoryViewModel.ListType,
                        Description = categoryViewModel.Text,
                        ImageId = categoryViewModel.CategoryImage,
                    });

                    repository.NewPage(new ContentPage
                    {
                        Name = categoryViewModel.Name,
                        Text = categoryViewModel.Text,
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

        public ActionResult Product(Guid? id)
        {
            var viewModel = id != null && id != Guid.Empty ? new ProductViewModel(this.repository, id.Value) : new ProductViewModel(this.repository);
            viewModel.InitializeRootCategory(this.repository);
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            return this.Authorize() ?? this.View(viewModel);
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

                return this.RedirectToAction("EditProducts", new { id = productViewModel.ParentCategoryId });
            }
            else
            {
                this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
                return this.Authorize() ?? this.View(productViewModel ?? new ProductViewModel(this.repository));
            }
        }

        public ActionResult ProductImage(Guid imageId)
        {
            this.ViewData["ImageId"] = imageId.ToString();
            return this.View("ProductImage", imageId);
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

        public ActionResult EditProducts(Guid id)
        {
            this.ViewData["CategoryTreeRootViewModel"] = CategoryTreeItemViewModel.CreateNavigationTree(repository);
            return this.Authorize() ?? this.View(new CategoryProductsViewModel(this.repository, id));
        }

        public ActionResult EditProductsPartial(Guid id)
        {
            return this.View("EditProductDetails", new CategoryProductsViewModel(this.repository, id));
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
    }
}