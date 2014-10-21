using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eve_api;
using Eve_Ship_ID.Models;

namespace Eve_Ship_ID.Controllers
{
    public class CorpSecurityController : Controller
    {
        //
        // GET: /CorpSecurity/

        public ActionResult Index()
        {
            var data = new CorpSecurityModel();

            if (this.HttpContext.Request.QueryString["refresh"] == "1")
            {
                data.RefreshCorpRoster(98340372);
            }


            data.PopulateCorpRoster(98340372);         

            return View(data);
        }

    }
}
