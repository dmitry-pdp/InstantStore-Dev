using InstantStore.Domain.Entities;
using System.Collections.Generic;

namespace InstantStore.Domain.Abstract
{
    public interface IRepository
    {
        IEnumerable<Product> Products { get; }
    }
}
