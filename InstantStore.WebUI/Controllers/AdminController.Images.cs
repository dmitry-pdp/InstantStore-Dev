using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Linq;
using System.IO;

using InstantStore.WebUI.HtmlHelpers;
using InstantStore.Domain.Concrete;
using System.Drawing.Drawing2D;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        [HttpPost]
        public ActionResult SaveImage(HttpPostedFileBase image)
        {
            if (image != null)
            {
                Binary imageBinary = image.GetFileBinary();
                if (imageBinary != null && !string.IsNullOrEmpty(image.ContentType))
                {
                    var imageId = this.repository.AddImage(new Image
                    {
                        Image1 = imageBinary,
                        ImageContentType = image.ContentType
                    });

                    return this.Json(new { ImageId = imageId.ToString() } );
                }
            }

            return this.HttpNotFound();
        }
    }
}