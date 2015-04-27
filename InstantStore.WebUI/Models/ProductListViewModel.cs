using InstantStore.Domain.Entities;
using System.Collections.Generic;

namespace InstantStore.WebUI.Models
{
    public class ProductListViewModel
    {
        public IEnumerable<Product1> Products { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string CurrentCategory { get; set; }

    }
}