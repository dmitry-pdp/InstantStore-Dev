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
        private Dictionary<string, string> metaTags;

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

        public Dictionary<string, string> MetaTags
        {
            get { return this.metaTags ?? (this.metaTags = this.CreateMetaTags()); }
        }

        public void ValidateAndSave()
        {
            // TODO: XSS and header consistency validation.
            this.repository.SetSettings(SettingsKey.HeaderHtml, this.headerHtml);
            this.repository.SetSettings(SettingsKey.FooterHtml, this.footerHtml);
            this.repository.SetSettings(SettingsKey.MainPageHtml, this.mainDocumentHtml);

            SaveMetaTag(SettingsKey.MetaTags_Description, "description");
            SaveMetaTag(SettingsKey.MetaTags_Keywords, "keywords");
            SaveMetaTag(SettingsKey.MetaTags_Copyright, "copyright");
            SaveMetaTag(SettingsKey.MetaTags_Robots, "robots");
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

        private Dictionary<string, string> CreateMetaTags()
        {
            return new Dictionary<string, string>
            {
                { "copyright", this.repository.GetSettings(SettingsKey.MetaTags_Copyright) ?? string.Empty },
                { "description", this.repository.GetSettings(SettingsKey.MetaTags_Description) ?? string.Empty },
                { "robots", this.repository.GetSettings(SettingsKey.MetaTags_Robots) ?? string.Empty },
                { "keywords", this.repository.GetSettings(SettingsKey.MetaTags_Keywords) ?? string.Empty }
            };
        }

        private void SaveMetaTag(SettingsKey key, string dictionaryKey)
        {
            if (this.metaTags != null && this.metaTags.ContainsKey(dictionaryKey))
            {
                this.repository.SetSettings(key, this.metaTags[dictionaryKey]);
            }
        }
    }
}