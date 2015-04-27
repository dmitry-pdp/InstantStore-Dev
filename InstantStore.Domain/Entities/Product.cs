
namespace InstantStore.Domain.Entities
{

    public class Product1
    {
        //All products should be also be added to some general category without parent category by default

        public int ProductID { get; set; }
        public string Name { get; set; }
        public ProductCategory Category { get; set; }
        public string Description { get; set; }
        public decimal PriceCash { get; set; }
        public decimal PriceСashless { get; set; }
        public ProductSize Size { get; set; }
        public int SortOrder { get; set; }        
    }
}
