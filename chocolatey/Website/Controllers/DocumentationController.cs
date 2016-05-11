using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuGetGallery.Controllers
{
    public class DocumentationController : Controller
    {
        //
        // GET: /Documentation/

        public ActionResult Index()
        {
            return View();
        }

        //todo: move these guys
        public ActionResult Install()
        {
            return View("~/Views/Pages/Install.cshtml");
        }

        public ActionResult FAQ()
        {
            return View("~/Views/Pages/FAQ.cshtml");
        }

        public ActionResult Security()
        {
            return View("~/Views/Pages/Security.cshtml");
        }
    }
}
