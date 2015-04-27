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
        public IList<PropertyTemplate> GetTemplates()
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.PropertyTemplates.Where(x => x.IsPrototype).ToList();
            }
        }

        public Guid AddNewTemplate(PropertyTemplate propertyTemplate)
        {
            using (var context = new InstantStoreDataContext())
            {
                var id = Guid.NewGuid();
                propertyTemplate.Id = id;
                propertyTemplate.IsPrototype = true;
                context.PropertyTemplates.InsertOnSubmit(propertyTemplate);
                context.SubmitChanges();
                return id;
            }
        }

        public PropertyTemplate GetTemplateById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.PropertyTemplates.FirstOrDefault(x => x.Id == id);
            }
        }

        public void DeleteTemplate(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                var template = context.PropertyTemplates.FirstOrDefault(x => x.Id == id);
                context.PropertyTemplates.DeleteOnSubmit(template);
                context.SubmitChanges();
            }
        }
     
        public IList<CustomProperty> GetPropertiesForTemplate(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.CustomProperties.Where(x => x.TemplateId == id).ToList();
            }
        }

        public Guid AddNewCustomProperty(CustomProperty customProperty)
        {
            using (var context = new InstantStoreDataContext())
            {
                if (!context.PropertyTemplates.Any(x => x.Id == customProperty.TemplateId))
                {
                    throw new ModelValidationException("Model.CustomProperty.TemplateNotFound");
                }
                
                customProperty.Id = Guid.NewGuid();
                context.CustomProperties.InsertOnSubmit(customProperty);
                context.SubmitChanges();

                return customProperty.Id;
            }
        }

        public void DeleteCustomProperty(Guid id)
        {
            // TODO: Check in the model if this property can be upated / deleted.

            using (var context = new InstantStoreDataContext())
            {
                var property = context.CustomProperties.FirstOrDefault(x => x.Id == id);
                if (property == null)
                {
                    throw new ModelValidationException("Model.CustomProperty.TemplateNotFound");
                }

                context.CustomProperties.DeleteOnSubmit(property);
                context.SubmitChanges();
            }
        }

        public void UpdateCustomProperty(Guid id, string data)
        {
            // TODO: Check in the model if this property can be upated / deleted.

            using (var context = new InstantStoreDataContext())
            {
                var property = context.CustomProperties.FirstOrDefault(x => x.Id == id);
                if (property == null)
                {
                    throw new ModelValidationException("Model.CustomProperty.TemplateNotFound");
                }

                property.Name = data;
                context.SubmitChanges();
            }
        }
        
        public void UpdateTemplate(PropertyTemplate propertyTemplate, IList<CustomProperty> customProperties, bool forceUpdate)
        {
            using (var context = new InstantStoreDataContext())
            {
                var template = context.PropertyTemplates.FirstOrDefault(x => x.Id == propertyTemplate.Id);
                if (template != null)
                {
                    template.Name = propertyTemplate.Name;

                    var properties = context.CustomProperties.Where(x => x.TemplateId == propertyTemplate.Id).ToList();
                    var propertiesToDelete = properties.Where(x => !customProperties.Any(y => y.Id == x.Id));
                    var propertiesToInsert = customProperties.Where(x => x.Id == Guid.Empty || !properties.Any(y => y.Id == x.Id));
                    var propertiesToUpdate = properties.Where(x => customProperties.Any(y => y.Id == x.Id));

                    foreach(var property in propertiesToInsert)
                    {
                        property.Id = Guid.NewGuid();
                        property.TemplateId = template.Id;
                        context.CustomProperties.InsertOnSubmit(property);
                    }

                    if (!forceUpdate)
                    {
                        // TODO: Check in the model if this property can be upated / deleted.
                    }

                    foreach (var property in propertiesToUpdate)
                    {
                        var p = customProperties.First(x => x.Id == property.Id);
                        property.Name = p.Name;
                    }

                    foreach (var property in propertiesToDelete)
                    {
                        context.CustomProperties.DeleteOnSubmit(property);
                    }

                    context.SubmitChanges();
                }
            }
        }
    }
}
