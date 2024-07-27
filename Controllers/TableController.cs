using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kubota.Web.Dashboard.Controllers
{
    public class TableController : Controller
    {
        // GET: Table
        public ActionResult TableIndex()
        {
            return View();
        }
    }
}