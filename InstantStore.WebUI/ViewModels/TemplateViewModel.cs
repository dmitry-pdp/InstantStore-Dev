using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using System.ComponentModel.DataAnnotations;

namespace InstantStore.WebUI.ViewModels
{
    public class TemplateViewModel
    {
        public TemplateViewModel()
        {
            this.Properties = new List<CustomProperty>();
        }
        public TemplateViewModel(IEnumerable<CustomProperty> properties)
        {
            this.Properties = properties.ToList();
        }

        public TemplateViewModel(PropertyTemplate template, IRepository repository)
        {
            this.Id = template.Id;
            this.Name = template.Name;
            this.Properties = repository.GetPropertiesForTemplate(this.Id);
        }

        public Guid Id { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "template_Name")]
        public string Name { get; set; }

        public IList<CustomProperty> Properties { get; private set; }
    }
}