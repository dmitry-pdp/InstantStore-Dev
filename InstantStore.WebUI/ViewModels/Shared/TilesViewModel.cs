using InstantStore.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class TilesViewModel
    {
        public IList<TilesViewModelGroup> TileGroups { get; set; }

        public PaginationViewModel Pagination { get; set; }
    }

    public class TilesViewModelGroup
    {
        public TilesViewModelGroup(TileGroupKey key, IEnumerable<TileViewModel> value)
        { 
            this.Key = key;
            this.Value = value;
        }

        public TileGroupKey Key { get; private set; }

        public IEnumerable<TileViewModel> Value { get; private set; }
    }
   
    public class TileGroupKey
    {
        public NavigationLink Link { get; set; }

        public string Title { get; set; }
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