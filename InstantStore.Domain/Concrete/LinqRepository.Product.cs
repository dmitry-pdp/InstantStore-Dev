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
                    context.Products.InsertOnSubmit(productToUpdate);

                    context.ContentPages.InsertOnSubmit(new ContentPage
                    {
                        Id = Guid.NewGuid(),
                        Name = productToUpdate.Name,
                        Text = productToUpdate.Description,
                        ParentId = parentId,
                        ContentType = (int)ContentType.Product,
                        Position = context.ContentPages.Count(p => p.ParentId == parentId) + 1,
                        ProductId = productToUpdate.VersionId
                    });

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

                    var page = context.ContentPages.FirstOrDefault(x => x.ProductId == productToUpdate.Id);
                    if (page == null)
                    {
                        throw new ModelValidationException("Model.State.Inconsistent");
                    }
                    
                    page.Name = productToUpdate.Name;
                    page.Text = productToUpdate.Description;
                    page.ParentId = parentId;

                    if (images != null)
                    {
                        var productImages = context.Images.Where(x => x.ProductId == product.Id).ToList();
                        var productImageIds = productImages.Select(x => x.Id);

                        var imageIdsToDelete = productImageIds.Except(images);
                        var imagesToDelete = productImages.Where(x => imageIdsToDelete.Contains(x.Id));

                        context.Images.DeleteAllOnSubmit(imagesToDelete);

                        var imageIdsToInsert = images.Except(productImageIds);
                        var imagesToInsert = context.Images.Where(x => imageIdsToInsert.Contains(x.Id));
                        foreach(var imageToUpdate in imagesToInsert)
                        {
                            imageToUpdate.ProductId = product.Id;
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
    }
}
