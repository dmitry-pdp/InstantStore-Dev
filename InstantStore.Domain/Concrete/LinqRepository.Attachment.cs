using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public Attachment GetAttachmentById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Attachments.FirstOrDefault(x => x.Id == id);
            }
        }

        public Guid AddAttachment(Attachment attachment)
        {
            using (var context = new InstantStoreDataContext())
            {
                var id = Guid.NewGuid();
                attachment.Id = id;
                context.Attachments.InsertOnSubmit(attachment);
                context.SubmitChanges();
                return id;
            }
        }
    }
}
