using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        [HttpPost]
        public ActionResult SaveImage(HttpPostedFileBase image)
        {
            if (image != null)
            {

            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}