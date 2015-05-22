using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InstantStore.WebUI.ViewModels
{
    public enum PaginationAction
    {
        Next, 
        Prev,
        Page
    };

    public class PaginationViewModel 
    {
        public PaginationViewModel()
        {
            this.UseJs = true;
        }

        public PaginationViewModel(int count, int offset, int max)
        {
            this.Count = count;
            this.CurrentPage = (offset / count);
            this.MaxPages = ((max - 1) / count);
        }

        public int CurrentPage { get; set; }

        public int Count { get; set; }

        public int MaxPages { get; set; }

        public string Id { get; set; }

        public bool UseJs { get; set; }

        public NavigationLink Link { get; set; }

        public string GetUrl(UrlHelper urlHelper, PaginationAction action, int page = 0)
        {
            var baseUrl = this.Link.GetUrl(urlHelper);
            var delim = baseUrl.Contains('?') ? '&' : '?';
            if (action != PaginationAction.Page)
            {
                page = Math.Min(this.MaxPages, Math.Max(0, (this.CurrentPage + (action == PaginationAction.Next ? +1 : -1))));
            }

            return string.Format("{0}{1}o={2}&c={3}", baseUrl, delim, page * this.Count, this.Count);
        }

        public IEnumerable<int> GetPages()
        {
            return Enumerable.Range(0, this.MaxPages + 1);
        }
    }
}