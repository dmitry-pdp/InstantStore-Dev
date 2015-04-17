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
    }
}
