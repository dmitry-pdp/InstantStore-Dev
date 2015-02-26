using InstantStore.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace InstantStore.Domain.Concrete
{
    public class LinqRepository : IRepository
    {
        //private InstantStoreDataContext context = new InstantStoreDataContext();

        public IEnumerable<Entities.Product> Products
        {
            get {
                IEnumerable<Entities.Product> result = null;
                try
                {
                    using (var context = new InstantStoreDataContext())
                    {
                        context.Connection.Open();
                        result = (from p in context.Products
                                  join c in context.ProductsCategories on p.ProductId equals c.ProductId
                                  join cat in context.ProductCategories on c.ProductCategoryID equals cat.ProductCategoryId
                                  select new Entities.Product
                                  {
                                      ProductID = p.ProductId,
                                      Name = p.Name,
                                      Category = new Entities.ProductCategory()
                                          {
                                              ProductCategoryID = cat.ProductCategoryId,
                                              ParentCategory = (cat.ParentCategory.HasValue ? new Entities.ProductCategory { ProductCategoryID = cat.ParentCategory.Value } : null),
                                              Name = cat.Name,
                                              Type = (Entities.CategoryDisplayType)cat.DisplayType,
                                              IsVisibleInMenu = cat.IsVisibleInMenu,
                                              URLAlias = cat.URLAlias
                                          },
                                      Description = p.Description,
                                      PriceCash = p.PriceCash,
                                      PriceСashless = p.PriceCashless,
                                      Size = new Entities.ProductSize { ProductSizeId = p.ProductSize.Id, Name = p.ProductSize.Name, SortOrder = p.ProductSize.SortOrder },
                                      SortOrder = c.SortOrder
                                  })
                                  .Distinct()
                                  .ToList();
                        if (context.Connection != null && context.Connection.State != System.Data.ConnectionState.Closed)
                            context.Connection.Close();
                    }
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message + ex.InnerException);
                }
                return result;                
            }
        }

    }
}
