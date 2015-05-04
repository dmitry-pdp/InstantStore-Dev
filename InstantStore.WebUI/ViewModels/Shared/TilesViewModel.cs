using InstantStore.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class TilesViewModel
    {
        public IList<TileViewModel> Tiles { get; set; }

        public PaginationViewModel Pagination { get; set; }
    }

    public class TileViewModel
    {
        public string Name { get; set; }

        public Guid Id { get; set; }
        
        public Guid ImageId { get; set; }
    }
}