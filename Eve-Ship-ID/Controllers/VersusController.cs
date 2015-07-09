using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eve_api;
using Eve_Ship_ID.Models;

namespace Eve_Ship_ID.Controllers
{
    public class VersusController : Controller
    {
        //
        // GET: /Versus/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(VersusModel vModel)
        {
            vModel.PopulateResultsForCharacterId(vModel.CharacterOneId);
            return View(vModel);
        }

        public string CharacterIdForName(string id)
        {
            return VersusModel.CharIdForName(id).ToString();
        }

    }
}
