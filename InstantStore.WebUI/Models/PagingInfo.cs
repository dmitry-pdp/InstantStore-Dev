using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.Models
{
    /// <summary>
    /// Information for view about the number of pages available, 
    /// the current page and the total amount of pages
    /// </summary>
    public class PagingInfo
    {
        public int TotalItems {get; set;}
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
        }
    }
}