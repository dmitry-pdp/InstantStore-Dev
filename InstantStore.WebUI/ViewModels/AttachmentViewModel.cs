using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class AttachmentViewModel
    {
        private static Dictionary<string, string> attachmentExtensionToIconMap = new Dictionary<string, string>
        {
            { "doc", "FileExtension-Word.png" },
            { "docx", "FileExtension-Word.png" },
            { "xls", "FileExtension-Excel.png" },
            { "xlsx", "FileExtension-Excel.png" },
            { "pdf", "FileExtension-pdf.png" },
            { "txt", "FileExtension-Txt.png" },
            { "jpg", "FileExtension-Jpg.png" },
            { "jpeg", "FileExtension-Jpg.png" }
        };

        public AttachmentViewModel()
        {
            this.CanEdit = true;
        }

        public AttachmentViewModel(Attachment attachment)
        {
            if (attachment != null && !string.IsNullOrEmpty(attachment.Name))
            {
                this.AttachmentId = attachment.Id;
                this.AttachmentName = attachment != null ? attachment.Name : null;
                string extension = this.AttachmentName != null ? this.AttachmentName.Substring(this.AttachmentName.LastIndexOf('.') + 1) : null;
                this.AttachmentIcon = extension != null && attachmentExtensionToIconMap.ContainsKey(extension) ? attachmentExtensionToIconMap[extension] : null;
                this.CanEdit = true;
            }
        }

        public AttachmentViewModel(ContentPageAttachment attachment)
        {
            if (attachment != null && !string.IsNullOrEmpty(attachment.Name))
            {
                this.AttachmentId = attachment.AttachmentId;
                this.AttachmentName = attachment != null ? attachment.Name : null;
                string extension = this.AttachmentName != null ? this.AttachmentName.Substring(this.AttachmentName.LastIndexOf('.') + 1) : null;
                this.AttachmentIcon = extension != null && attachmentExtensionToIconMap.ContainsKey(extension) ? attachmentExtensionToIconMap[extension] : null;
                this.CanEdit = true;
            }
        }

        public Guid? AttachmentId { get; set; }

        public string AttachmentName { get; set; }

        public string AttachmentIcon { get; set; }

        public bool CanEdit { get; set; }
    }
}