using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public IList<ContentPage> GetPages(Guid? parentId)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.ContentPages.Where(x => parentId != null ? x.ParentId == parentId : x.ParentId == null).ToList();
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
    }
}
