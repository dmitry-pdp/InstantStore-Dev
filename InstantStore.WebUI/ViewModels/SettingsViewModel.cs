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
            // TODO: XSS and header consistency validation.
            this.repository.SetSettings(SettingsKey.HeaderHtml, this.headerHtml);
            this.repository.SetSettings(SettingsKey.FooterHtml, this.footerHtml);
            this.repository.SetSettings(SettingsKey.MainPageHtml, this.mainDocumentHtml);
        }

        private string CreateHeaderHtml()
        {
            return this.repository.GetSettings(SettingsKey.HeaderHtml) ?? StringResource.Header_Empty;
        }

        private string CreateFooterHtml()
        {
            return this.repository.GetSettings(SettingsKey.FooterHtml) ?? string.Empty;
        }

        private string CreateMainDocumentHtml()
        {
            return this.repository.GetSettings(SettingsKey.MainPageHtml) ?? string.Empty;
        }
    }
}