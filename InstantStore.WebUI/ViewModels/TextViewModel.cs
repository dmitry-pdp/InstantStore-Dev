using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class CustomViewModel
    {
        public string Title { get; set; }

        public string ViewName { get; set; }
    }

    public class TextViewModel : CustomViewModel
    {
        public Guid Id { get; set; }

        public bool HasSubject { get; set; }

        public bool HasRichText { get; set; }

        public string Subject { get; set; }

        public string SubjectLabel { get; set; }

        public string Content { get; set; }

        public string ContentLabel { get; set; }
    }

    public class PropertyInfo
    {
        public PropertyInfo()
        {
        }

        public PropertyInfo(string key, string label, string value)
        {
            this.Key = key;
            this.Label = label;
            this.Value = value;
        }

        public string Key { get; set; }

        public string Label { get; set; }

        public string Value { get; set; }

        public bool IsMultiLine { get; set; }
    }

    public class PropertyListViewModel : CustomViewModel
    {
        public IList<PropertyInfo> Properties { get; set; }

        public string CustomText { get; set; }
    }
}