using InstantStore.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public IList<ContentPage> GetPages(Guid? parentId, Func<ContentPage, bool> filter)
        {
            using (var context = new InstantStoreDataContext())
            {
                var result = context.ContentPages.Where(x => (parentId != null ? x.ParentId == parentId : x.ParentId == null));
                return filter != null ? result.Where(filter).ToList() : result.ToList();
            }
        }

        public Guid NewPage(ContentPage contentPage)
        {
            using (var context = new InstantStoreDataContext())
            {
                contentPage.Id = Guid.NewGuid();
                UpdateAttachmentName(contentPage, context);

                context.ContentPages.InsertOnSubmit(contentPage);
                context.SubmitChanges();
                return contentPage.Id;
            }
        }

        public void DeletePage(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                var page = context.ContentPages.FirstOrDefault(x => x.Id == id);
                if (page != null)
                {
                    if (page.AttachmentId != null)
                    {
                        context.Attachments.DeleteAllOnSubmit(context.Attachments.Where(a => a.Id == page.AttachmentId));
                    }

                    context.ContentPages.DeleteOnSubmit(page);
                    context.SubmitChanges();
                }
            }
        }

        private static void UpdateAttachmentName(ContentPage contentPage, InstantStoreDataContext context)
        {
            if (contentPage.AttachmentId != null)
            {
                contentPage.AttachmentName = context.Attachments
                    .Where(x => x.Id == contentPage.AttachmentId)
                    .Select(x => x.Name)
                    .First();
            }

        }

        public void UpdateContentPage(ContentPage contentPage)
        {
            using (var context = new InstantStoreDataContext())
            {
                var contentPageOriginal = context.ContentPages.FirstOrDefault(x => x.Id == contentPage.Id);
                if (contentPageOriginal == null)
                {
                    throw new ModelValidationException("UpdateContentPage.OriginalPageDoesNotExists");
                }

                contentPageOriginal.Name = contentPage.Name;
                contentPageOriginal.Text = contentPage.Text;
                contentPageOriginal.ParentId = contentPage.ParentId;
                contentPageOriginal.AttachmentId = contentPage.AttachmentId;
                UpdateAttachmentName(contentPageOriginal, context);

                context.SubmitChanges();
            }
        }

        public void ChangePagePosition(Guid id, bool movedown)
        {
            using (var context = new InstantStoreDataContext())
            {
                var page = context.ContentPages.FirstOrDefault(x => x.Id == id);
                if (page == null)
                {
                    throw new ModelValidationException("UpdateContentPage.OriginalPageDoesNotExists");
                }

                if (page.Position == 0 && !movedown)
                {
                    return;
                }

                var siblingPages = context.ContentPages.Where(x => page.ParentId != null ? x.ParentId == page.ParentId : x.ParentId == null && x.Id != page.Id).ToList();
                if (page.Position >= siblingPages.Count && movedown)
                {
                    return;
                }

                int newPosition = page.Position + (movedown ? 1 : -1);

                var exchangePage = siblingPages.FirstOrDefault(x => x.Position == newPosition);
                if (exchangePage != null)
                {
                    exchangePage.Position = page.Position;
                }

                page.Position = newPosition;

                context.SubmitChanges();
            }
        }

        public ContentPage GetPageById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.ContentPages.FirstOrDefault(x => x.Id == id);
            }
        }

        public ContentPage GetPageByCategoryId(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.ContentPages.FirstOrDefault(x => x.CategoryId != null && x.CategoryId == id);
            }
        }

        public ContentPage GetPageByProductId(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.ContentPages.FirstOrDefault(x => x.ProductId != null && x.ProductId == id);
            }
        }

        public Guid NewCategory(Category category)
        {
            using (var context = new InstantStoreDataContext())
            {
                category.Id = Guid.NewGuid();
                category.VersionId = Guid.NewGuid();
                context.Categories.InsertOnSubmit(category);
                context.SubmitChanges();
                return category.VersionId;
            }
        }

        public Category GetCategoryById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Categories.FirstOrDefault(x => x.VersionId == id);
            }
        }

        public void UpdateCategory(Category category)
        {
            using (var context = new InstantStoreDataContext())
            {
                var categoryOriginal = context.Categories.FirstOrDefault(x => x.Id == category.Id);
                if (categoryOriginal == null)
                {
                    throw new ModelValidationException("UpdateContentPage.OriginalPageDoesNotExists");
                }

                categoryOriginal.Name = category.Name;
                categoryOriginal.Description = category.Description;
                categoryOriginal.ImageId = category.ImageId;
                categoryOriginal.ListType = category.ListType;
                categoryOriginal.ShowInMenu = category.ShowInMenu;
                context.SubmitChanges();
            }

        }

        public Guid NewProduct(Product product)
        {
            using (var context = new InstantStoreDataContext())
            {
                product.Id = Guid.NewGuid();
                product.VersionId = Guid.NewGuid();
                context.Products.InsertOnSubmit(product);
                context.SubmitChanges();
                return product.VersionId;
            }
        }

        public Product GetProductById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Products.FirstOrDefault(x => x.VersionId == id);
            }
        }

        public IList<Guid> GetImagesForProduct(Guid productId)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Images.Where(x => x.ProductId == productId).Select(x => x.Id).ToList();
            }
        }

        public void AssignImagesToProduct(Guid productId, IEnumerable<Guid> images)
        {
            if (images == null)
            {
                throw new ArgumentNullException("images");
            }

            using (var context = new InstantStoreDataContext())
            {
                foreach(var imageId in images)
                {
                    var image = context.Images.FirstOrDefault(x => x.Id == imageId);
                    if (image != null)
                    {
                        image.ProductId = productId;
                    }
                }

                context.SubmitChanges();
            }
        }

        public IList<Product> GetProductsForCategory(Guid categoryId, int page, int count)
        {
            using (var context = new InstantStoreDataContext())
            {
                var products = this.GetProducts(context, categoryId);
                return context.Products.Where(x => products.Any(y => x.VersionId == y)).Skip(count * page).Take(count).ToList();
            }
        }

        public int GetProductsCountForCategory(Guid categoryId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var products = this.GetProducts(context, categoryId);
                return context.Products.Where(x => products.Any(y => x.VersionId == y)).Count();
            }
        }

        private IQueryable<Guid> GetProducts(InstantStoreDataContext context, Guid categoryId)
        {
            var products = context.ContentPages.Where(x => x.ProductId != null && x.ContentType == 3 && x.ParentId != null && x.ParentId == categoryId && x.ProductId != null).Select(x => x.ProductId.Value);
            return products.Union(context.ProductToCategories.Where(x => x.CategoryId == categoryId).Select(x => x.ProductId));
        }
    }
}
