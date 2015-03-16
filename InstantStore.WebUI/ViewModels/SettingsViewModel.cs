using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels
{
    public class SettingsViewModel
    {
        private readonly IRepository repository;
        private string headerHtml;
        private string footerHtml;
        private string mainDocumentHtml;

        public SettingsViewModel(IRepository repository)
        {
            this.repository = repository;
        }

        public string HeaderHtml 
        {
            get { return this.headerHtml ?? (this.headerHtml = this.CreateHeaderHtml()); }
            set { this.headerHtml = value; }
        }

        public string FooterHtml
        {
            get { return this.footerHtml ?? (this.footerHtml = this.CreateFooterHtml()); }
            set { this.footerHtml = value; }
        }

        public string MainDocumentHtml
        {
            get { return this.mainDocumentHtml ?? (this.mainDocumentHtml = this.CreateMainDocumentHtml()); }
            set { this.mainDocumentHtml = value; }
        }

        public void ValidateAndSave()
        {
            var setting = this.repository.Settings;
            if (setting == null)
            {
                setting = new Setting();
            }

            // TODO: XSS and header consistency validation.

            setting.HeaderHtml = this.headerHtml;
            setting.FooterHtml = this.footerHtml;
            setting.MainDescription = this.mainDocumentHtml;
            
            this.repository.Update(setting);
        }

        private string CreateHeaderHtml()
        {
            if (this.repository.Settings != null && !string.IsNullOrEmpty(this.repository.Settings.HeaderHtml))
            {
                return this.repository.Settings.HeaderHtml;
            }
            else
            {
                return StringResource.Header_Empty;
            }
        }

        private string CreateFooterHtml()
        {
            if (this.repository.Settings != null && !string.IsNullOrEmpty(this.repository.Settings.FooterHtml))
            {
                return this.repository.Settings.FooterHtml;
            }
            else
            {
                return string.Empty;
            }
        }

        private string CreateMainDocumentHtml()
        {
            if (this.repository.Settings != null && !string.IsNullOrEmpty(this.repository.Settings.MainDescription))
            {
                return this.repository.Settings.MainDescription;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}