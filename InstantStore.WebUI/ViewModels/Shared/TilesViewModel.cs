using InstantStore.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class TilesViewModel
    {
        public IList<KeyValuePair<string, IList<TileViewModel>>> TileGroups { get; set; }

        public PaginationViewModel Pagination { get; set; }
    }

    public class AttributeList : List<KeyValuePair<string, string>>
    {
    }

    public class TileViewModel
    {
        public string Name { get; set; }

        public NavigationLink Link { get; set; }
        
        public Guid ImageId { get; set; }

        public AttributeList Attributes { get; set; }

        public NavigationLink Action { get; set; }
    }
}