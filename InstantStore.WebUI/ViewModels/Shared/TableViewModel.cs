using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class TableViewModel : CustomViewModel
    {
        public TableViewModel()
        {
            this.Id = Guid.NewGuid().GetHashCode().ToString();
        }

        public IList<TableRowViewModel> Rows { get; set; }

        public IList<TableCellViewModel> Header { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public string Id { get; private set; }

        public NavigationLink RowClickAction { get; set; }

        public bool HasCheckboxes { get; set; }
    }

    public class TableRowViewModel
    {
        public string Id { get; set; }

        public string ParentId { get; set; }

        public TableCellViewModel GroupCell { get; set; }

        public IList<TableCellViewModel> Cells { get; set; }
    }

    public class TableCellViewModel
    {
        public TableCellViewModel(string text)
        {
            this.Text = text;
        }

        public TableCellViewModel(NavigationLink action)
        {
            this.Action = action;
        }

        public TableCellViewModel(ImageThumbnailViewModel thumbnail)
        {
            this.Thumbnail = thumbnail;
        }

        public string Text { get; set; }

        public NavigationLink Action { get; set; }

        public ImageThumbnailViewModel Thumbnail { get; set; }
    }
}