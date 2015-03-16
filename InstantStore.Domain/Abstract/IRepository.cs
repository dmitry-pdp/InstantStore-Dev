using InstantStore.Domain.Concrete;
using InstantStore.Domain.Entities;
using System.Collections.Generic;

namespace InstantStore.Domain.Abstract
{
    public interface IRepository
    {
        IEnumerable<InstantStore.Domain.Entities.Product> Products { get; }

        Setting Settings { get; }

        void Update(Setting settings);

        void AddFeedback(Feedback feedback);

        void AddUser(User user);
    }
}
