using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InstantStore.Domain.Entities;
using InstantStore.Domain.Exceptions;
using InstantStore.Domain.Helpers;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public void UpdateOrCreateNewProduct(Product productToUpdate, Guid parentId, IList<Guid> images, Guid? prototypeTemplateId, IList<CustomProperty> attributes, int position)
        {
            using(var context = new InstantStoreDataContext())
            {
                var product = productToUpdate.Id != Guid.Empty ? context.Products.FirstOrDefault(x => x.VersionId == productToUpdate.Id) : null;
                if (product == null)
                {
                    productToUpdate.Id = Guid.NewGuid();
                    productToUpdate.VersionId = Guid.NewGuid();
                    productToUpdate.MainImageId = images != null && images.Any() ? images.First() : (Guid?)null;

                    context.Products.InsertOnSubmit(productToUpdate);

                    context.SubmitChanges();

                    product = productToUpdate;
                 
                    while (parentId != Guid.Empty)
                    {
                        var parent = context.ContentPages.First(x => x.Id == parentId);
                        if (parent.CategoryId != null)
                        {
                            context.ProductToCategories.InsertOnSubmit(new ProductToCategory
                            {
                                Id = Guid.NewGuid(),
                                CategoryId = parentId,
                                ProductId = productToUpdate.VersionId,
                                UpdateTime = DateTime.Now,
                                Index = position
                            });

                            context.SubmitChanges();

                            CategoryTreeBuilder.RebuidCategoryTreeGroups(context, parentId);
                            break;
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

                    var productImages = context.Images.Where(x => x.ProductId == product.Id).ToList();
                    var productImageIds = productImages.Select(x => x.Id);
                    var productThumbnails = context.ImageThumbnails.Where(x => productImageIds.Contains(x.Id)).ToList();

                    if (images != null)
                    {
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
                    else
                    {
                        var imageIdsToDelete = productImageIds;
                        var imagesToDelete = productImages.Where(x => imageIdsToDelete.Contains(x.Id));
                        var thumbnailsToDelete = productThumbnails.Where(x => imageIdsToDelete.Contains(x.Id));

                        context.Images.DeleteAllOnSubmit(imagesToDelete);
                        context.ImageThumbnails.DeleteAllOnSubmit(thumbnailsToDelete);

                        product.MainImageId = null;
                    }

                    if (product.CustomAttributesTemplateId != null && (prototypeTemplateId == null || prototypeTemplateId == Guid.Empty))
                    {
                        product.CustomAttributesTemplateId = null;
                    }

                    AddAttributes(product, prototypeTemplateId, attributes, context);
                }

                var productPrimaryCategories = context.ProductToCategories.Where(x => x.ProductId == product.VersionId && x.CategoryId == parentId);
                foreach (var productPrimaryCategory in productPrimaryCategories)
                {
                    productPrimaryCategory.Index = position;
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

        public IList<CustomProperty> CreateAttributesForProduct(Guid templateId)
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

                CategoryTreeBuilder.RebuidCategoryTreeGroups(context, contentPageId);
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

        public IList<Tuple<string, Guid?, Product>> GetProductsForCategory(Guid categoryId, int offset, int count)
        {
            using (var context = new InstantStoreDataContext())
            {
                return GetProductsForCategory(context, categoryId, offset, count);
            }
        }

        public static IList<Tuple<string, Guid?, Product>> GetProductsForCategory(InstantStoreDataContext context, Guid categoryId, int offset, int count)
        {
            return context.Products
                .Join(
                    GetProducts(context, categoryId),
                    a => a.VersionId,
                    b => b.ProductId,
                    (Product x, ProductToCategory y) => new 
                    { 
                        Product = x, 
                        GroupName = y.Group != null ? y.Group.Name : null, 
                        GroupId = y.GroupId,
                        Position = y.Group != null ? y.Group.Position : 1,
                        Index = y.Index
                    })
                .OrderBy(x => x.Position * 100000 + x.Index)
                .Skip(offset)
                .Take(count)
                .ToList()
                .Select(x => new Tuple<string, Guid?, Product>(x.GroupName, x.GroupId, x.Product))
                .ToList();
        }

        public int GetProductsCountForCategory(Guid categoryId)
        {
            using (var context = new InstantStoreDataContext())
            {
                return GetProductsCountForCategory(context, categoryId);
            }
        }

        public static int GetProductsCountForCategory(InstantStoreDataContext context, Guid categoryId)
        {
            var products = GetProducts(context, categoryId);
            return context.Products.Where(x => products.Any(y => x.VersionId == y.ProductId)).Count();
        }

        public IList<Product> GetProductsByPopularity(int count)
        {
            using (var context = new InstantStoreDataContext())
            { 
                // TODO: query top products in orders.
                return context.Products.Where(x => x.MainImageId != null).Take(count).ToList();
            }
        }

        public void RemoveProductFromCategory(Guid categoryId, IList<Guid> productIdsToRemove)
        {
            using (var context = new InstantStoreDataContext())
            {
                if (productIdsToRemove != null && productIdsToRemove.Any())
                {
                    foreach (var productId in productIdsToRemove)
                    {
                        var productMappings = context.ProductToCategories.Where(x => x.CategoryId == categoryId && productId == x.ProductId && x.GroupId == null);
                        context.ProductToCategories.DeleteAllOnSubmit(productMappings);
                    }

                    context.SubmitChanges();

                    CategoryTreeBuilder.RebuidCategoryTreeGroups(context, categoryId);
                }
            }
        }

        private static IQueryable<ProductToCategory> GetProducts(InstantStoreDataContext context, Guid categoryId)
        {
            return context.ProductToCategories.Where(x => x.CategoryId == categoryId);
        }

        public Guid CloneProduct(Guid productId, Guid parentId)
        {
            if (productId == Guid.Empty)
            {
                return Guid.Empty;
            }

            using (var context = new InstantStoreDataContext())
            {
                var product = context.Products.FirstOrDefault(x => x.VersionId == productId);
                if (product == null)
                {
                    throw new ModelValidationException("Model.Invalid:ProductDoesNotExists");
                }

                var pageCategory = context.ContentPages.FirstOrDefault(x => x.Id == parentId);
                if (pageCategory == null)
                {
                    throw new ModelValidationException("Model.Invalid:ContentPageDoesNotExists");
                }

                if (!pageCategory.IsCategory())
                {
                    throw new ModelValidationException("Model.Invalid:PageIsNotCategory");
                }

                // Build the clone

                var clone = new Product()
                {
                    Id = Guid.NewGuid(),
                    CashAccepted = product.CashAccepted,
                    Currency = product.Currency,
                    CustomAttributesTemplateId = product.CustomAttributesTemplateId,
                    Description = product.Description,
                    Image = product.Image,
                    IsAvailable = product.IsAvailable,
                    MainImageId = product.MainImageId,
                    Name = product.Name,
                    PriceCurrencyId = product.PriceCurrencyId,
                    PriceValueCash = product.PriceValueCash,
                    PriceValueCashless = product.PriceValueCashless,
                    PropertyTemplate = product.PropertyTemplate,
                    Version = 1,
                    VersionId = Guid.NewGuid()
                };

                const string endingStirng = " - Копия ";
                int cloneEndingIndex = clone.Name.IndexOf(endingStirng);
                if (cloneEndingIndex > -1)
                {
                    int indexValue = 0;
                    string indexString = clone.Name.Substring(cloneEndingIndex + endingStirng.Length);
                    if (!string.IsNullOrEmpty(indexString) && int.TryParse(indexString, out indexValue))
                    {
                        clone.Name = clone.Name.Substring(0, cloneEndingIndex) + endingStirng + indexValue.ToString();
                    }
                    else
                    {
                        clone.Name = clone.Name + endingStirng + "1";
                    }
                }
                else
                {
                    clone.Name = clone.Name + endingStirng + "1";
                }

                // Updating the tables

                context.Products.InsertOnSubmit(clone);

                // Clone images

                var images = context.Images.Where(x => x.ProductId == product.Id);
                if (images.Any())
                {
                    foreach(var image in images)
                    {
                        var clonedImage = new Image {
                            Id = Guid.NewGuid(),
                            ProductId = clone.Id,
                            ImageContentType = image.ImageContentType,
                            Image1 = image.Image1
                        };

                        context.Images.InsertOnSubmit(clonedImage);

                        var thumbnail = context.ImageThumbnails.FirstOrDefault(x => x.Id == image.Id);
                        if (thumbnail != null)
                        {
                            var clonedThumbnail = new ImageThumbnail {
                                Id = clonedImage.Id,
                                Image = clonedImage,
                                LargeThumbnail = thumbnail.LargeThumbnail,
                                SmallThumbnail = thumbnail.SmallThumbnail
                            };

                            context.ImageThumbnails.InsertOnSubmit(clonedThumbnail);
                        }
                    }
                }

                // Place product in to the same category as original.

                context.ProductToCategories.InsertOnSubmit(new ProductToCategory
                {
                    Id = Guid.NewGuid(),
                    CategoryId = parentId,
                    ProductId = clone.VersionId,
                    UpdateTime = DateTime.Now
                });

                CategoryTreeBuilder.RebuidCategoryTreeGroups(context, parentId);
                context.SubmitChanges();

                return clone.VersionId;
            }
        }

        public int GetProductPosition(Guid id, Guid parentId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var productCategory = context.ProductToCategories.Where(x => x.Product.VersionId == id && x.CategoryId == parentId).FirstOrDefault();
                return productCategory == null ? -1 : productCategory.Index;
            }
        }
    }

    public static class LinqRepositoryExtensions
    {
        public static int GetProductsCountForCategory(this InstantStoreDataContext context, Guid categoryId)
        {
            return LinqRepository.GetProductsCountForCategory(context, categoryId);
        }

        public static IList<Tuple<string, Guid?, Product>> GetProductsForCategory(this InstantStoreDataContext context, Guid categoryId, int offset, int count)
        {
            return LinqRepository.GetProductsForCategory(context, categoryId, offset, count);
        }
    }
}
