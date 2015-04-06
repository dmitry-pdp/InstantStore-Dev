using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.HtmlHelpers
{
    public static class ControllerHelpers
    {
        public static Binary GetFileBinary(this HttpPostedFileBase file)
        {
            if (file == null)
            {
                return null;
            }

            var buffer = new byte[file.ContentLength];
            file.InputStream.Read(buffer, 0, file.ContentLength);
            return new Binary(buffer);
        }
    }
}