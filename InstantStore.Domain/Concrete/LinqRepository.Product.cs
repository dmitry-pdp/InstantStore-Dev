using InstantStore.Domain.Entities;
using InstantStore.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public void UpdateOrCreateNewProduct(Product productToUpdate, Guid parentId, IList<Guid> images, Guid? prototypeTemplateId, IList<CustomProperty> attributes)
        {
            using(var context = new InstantStoreDataContext())
            {
                var product = productToUpdate.Id != Guid.Empty ? context.Products.FirstOrDefault(x => x.VersionId == productToUpdate.Id) : null;
                if (product == null)
                {
                    productToUpdate.Id = Guid.NewGuid();
                    productToUpdate.VersionId = Guid.NewGuid();
                    productToUpdate.MainImageId = images.Any() ? images.First() : (Guid?)null;

                    context.Products.InsertOnSubmit(productToUpdate);
                                        
                    while(parentId != Guid.Empty)
                    {
                        var parent = context.ContentPages.First(x => x.Id == parentId);
                        if (parent.CategoryId != null)
                        {
                            context.ProductToCategories.InsertOnSubmit(new ProductToCategory
                            {
                                Id = Guid.NewGuid(),
                                CategoryId = parentId,
                                ProductId = productToUpdate.VersionId,
                                UpdateTime = DateTime.Now
                            });
                        }

                        parentId = parent.ParentId ?? Guid.Empty;
                    }

                    if (images != null)
                    {
                        foreach (var imageId in images)
                        {
                            var image = context.Images.FirstOrDefault(x => x.Id == imageId);
                            if (image != null)
                            {
                                image.ProductId = productToUpdate.Id;
                            }
                        }
                    }

                    AddAttributes(productToUpdate, prototypeTemplateId, attributes, context);
                }
                else 
                {
                    product.Name = productToUpdate.Name;
                    product.Description = productToUpdate.Description;
                    product.IsAvailable = productToUpdate.IsAvailable;
                    product.PriceCurrencyId = productToUpdate.PriceCurrencyId;
                    product.PriceValueCash = productToUpdate.PriceValueCash;
                    product.PriceValueCashless = productToUpdate.PriceValueCashless;

                    if (images != null)
                    {
                        var productImages = context.Images.Where(x => x.ProductId == product.Id).ToList();
                        var productImageIds = productImages.Select(x => x.Id);
                        var productThumbnails = context.ImageThumbnails.Where(x => productImageIds.Contains(x.Id)).ToList();

                        var imageIdsToDelete = productImageIds.Except(images);
                        var imagesToDelete = productImages.Where(x => imageIdsToDelete.Contains(x.Id));
                        var thumbnailsToDelete = productThumbnails.Where(x => imageIdsToDelete.Contains(x.Id));

                        context.Images.DeleteAllOnSubmit(imagesToDelete);
                        context.ImageThumbnails.DeleteAllOnSubmit(thumbnailsToDelete);

                        var imageIdsToInsert = images.Except(productImageIds);
                        var imagesToInsert = context.Images.Where(x => imageIdsToInsert.Contains(x.Id));
                        foreach(var imageToUpdate in imagesToInsert)
                        {
                            imageToUpdate.ProductId = product.Id;
                        }

                        if (product.MainImageId == null || product.MainImageId == Guid.Empty || imageIdsToDelete.Contains(product.MainImageId.Value))
                        {
                            product.MainImageId = imageIdsToInsert.Any() ? imageIdsToInsert.First() : (Guid?)null;
                        }
                    }

                    if (product.CustomAttributesTemplateId != null && (prototypeTemplateId == null || prototypeTemplateId == Guid.Empty))
                    {
                        product.CustomAttributesTemplateId = null;
                    }

                    AddAttributes(product, prototypeTemplateId, attributes, context);
                }
                
                context.SubmitChanges();
            }
        }

        private static void AddAttributes(Product product, Guid? prototypeTemplateId, IList<CustomProperty> attributes, InstantStoreDataContext context)
        {
            if (prototypeTemplateId != null && prototypeTemplateId != Guid.Empty && attributes != null)
            {
                var template = context.PropertyTemplates.FirstOrDefault(x => x.Id == prototypeTemplateId);
                if (template == null)
                {
                    throw new ModelValidationException("Model.Inconsistent.PropertyTemplate");
                }

                var productAttributes = new PropertyTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = template.Name,
                    IsPrototype = false,
                    PrototypeId = prototypeTemplateId
                };

                context.PropertyTemplates.InsertOnSubmit(productAttributes);

                var templateAttributes = context.CustomProperties.Where(x => x.TemplateId == template.Id);

                foreach (var templateAttribute in templateAttributes)
                {
                    var productAttribute = attributes.FirstOrDefault(a => a.Name == templateAttribute.Name);
                    productAttribute.Id = Guid.NewGuid();
                    productAttribute.TemplateId = productAttributes.Id;
                    context.CustomProperties.InsertOnSubmit(productAttribute);
                }

                product.CustomAttributesTemplateId = productAttributes.Id;
            }
        }

        public IList<CustomProperty> CreateAttributesForProduct(Guid productId, Guid templateId)
        {
            using (var context = new InstantStoreDataContext())
            {
                if (!context.PropertyTemplates.Any(x => x.Id == templateId))
                {
                    throw new ModelValidationException("Model.InvalidRequest");
                }

                var templateProperties = context.CustomProperties.Where(x => x.TemplateId == templateId).ToList();
                var properties = templateProperties.Select(property => new CustomProperty()
                {
                    Id = Guid.NewGuid(),
                    Name = property.Name,
                    TemplateId = Guid.Empty,
                    Value = string.Empty
                });

                return properties.ToList();                
            }
        }

        public void AssignProductsToCategory(IList<Guid> products, Guid contentPageId)
        {
            using (var context = new InstantStoreDataContext())
            {
                context.ProductToCategories.InsertAllOnSubmit(products.Select(x => new ProductToCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = x,
                    CategoryId = contentPageId,
                    UpdateTime = DateTime.Now
                }));

                // assign to all parents

                context.SubmitChanges();
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

        public Product GetProductById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Products.FirstOrDefault(x => x.VersionId == id);
            }
        }

        public IList<Guid> GetImagesForProduct(Guid productId)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Images.Where(x => x.ProductId == productId).Select(x => x.Id).ToList();
            }
        }

        public void AssignImagesToProduct(Guid productId, IEnumerable<Guid> images)
        {
            if (images == null)
            {
                throw new ArgumentNullException("images");
            }

            using (var context = new InstantStoreDataContext())
            {
                foreach (var imageId in images)
                {
                    var image = context.Images.FirstOrDefault(x => x.Id == imageId);
                    if (image != null)
                    {
                        image.ProductId = productId;
                    }
                }

                context.SubmitChanges();
            }
        }

        public IList<Product> GetProductsForCategory(Guid categoryId, int offset, int count)
        {
            using (var context = new InstantStoreDataContext())
            {
                var products = this.GetProducts(context, categoryId);
                return context.Products.Where(x => products.Any(y => x.VersionId == y)).Skip(offset).Take(count).ToList();
            }
        }

        public int GetProductsCountForCategory(Guid categoryId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var products = this.GetProducts(context, categoryId);
                return context.Products.Where(x => products.Any(y => x.VersionId == y)).Count();
            }
        }

        public IList<Product> GetProductsByPopularity(int count)
        {
            using (var context = new InstantStoreDataContext())
            { 
                // TODO: query top products in orders.
                return context.Products.Where(x => x.MainImageId != null).Take(count).ToList();
            }
        }

        private IQueryable<Guid> GetProducts(InstantStoreDataContext context, Guid categoryId)
        {
            return context.ProductToCategories.Where(x => x.CategoryId == categoryId).Select(x => x.ProductId);
        }
    }
}
