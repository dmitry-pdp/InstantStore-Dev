using InstantStore.Domain.Exceptions;
using InstantStore.Domain.Helpers;
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

                    if (page.ParentId != null)
                    {
                        CategoryTreeBuilder.RebuidCategoryTreeGroups(context, page.ParentId.Value);
                    }
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

                Guid? oldParentId = contentPageOriginal.ParentId;

                contentPageOriginal.Name = contentPage.Name;
                contentPageOriginal.Text = contentPage.Text;
                contentPageOriginal.ParentId = contentPage.ParentId;
                contentPageOriginal.AttachmentId = contentPage.AttachmentId;
                contentPageOriginal.ShowInMenu = contentPage.ShowInMenu;
                UpdateAttachmentName(contentPageOriginal, context);

                context.SubmitChanges();

                if (contentPageOriginal.IsCategory())
                {
                    if (oldParentId != contentPageOriginal.ParentId && oldParentId != null)
                    {
                        // Update the old parent category
                        CategoryTreeBuilder.UpdateCategoryGropus(oldParentId.Value, context);

                        // Update both tree nodes & up for old and new parents.
                        CategoryTreeBuilder.RebuidCategoryTreeGroups(context, oldParentId.Value);
                    }

                    if (contentPageOriginal.ParentId != null)
                    {
                        CategoryTreeBuilder.RebuidCategoryTreeGroups(context, contentPageOriginal.Id);
                    }
                }
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
                var categoryId = context.ProductToCategories.Where(x => x.ProductId == id).Select(x => x.CategoryId).First();
                return context.ContentPages.FirstOrDefault(x => x.CategoryId != null && x.CategoryId == categoryId);
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
                categoryOriginal.IsImportant = category.IsImportant;
                context.SubmitChanges();
            }
        }

        public IList<Category> GetPriorityCategories()
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Categories.Where(x => x.IsImportant).ToList();
            }
        }
    }
}
