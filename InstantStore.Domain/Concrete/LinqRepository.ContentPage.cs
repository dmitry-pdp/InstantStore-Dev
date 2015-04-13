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
                return context.ContentPages.Where(x => (parentId != null ? x.ParentId == parentId : x.ParentId == null)).Where(filter).ToList();
            }
        }

        public Guid NewPage(ContentPage contentPage)
        {
            using (var context = new InstantStoreDataContext())
            {
                contentPage.Id = Guid.NewGuid();
                context.ContentPages.InsertOnSubmit(contentPage);
                context.SubmitChanges();
                return contentPage.Id;
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
