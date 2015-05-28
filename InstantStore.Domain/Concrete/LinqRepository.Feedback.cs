using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public void AddFeedback(Feedback feedback)
        {
            using (var context = new InstantStoreDataContext())
            {
                feedback.Id = Guid.NewGuid();
                feedback.Submitted = DateTime.Now;
                context.Feedbacks.InsertOnSubmit(feedback);
                context.SubmitChanges();
            }
        }
    }
}
