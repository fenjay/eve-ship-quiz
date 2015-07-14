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
            vModel.CharacterOneName = vModel.CharacterOneName.Trim();
            vModel.CharacterTwoName = vModel.CharacterTwoName.Trim();

            vModel.PopulateResultsForCharacterId(vModel.CharacterOneId, vModel.CharacterTwoId);
            return View(vModel);
        }

        public string CharacterIdForName(string id)
        {
            if (id == string.Empty)   //let's avoid unnecessary API calls
            { 
                return "0"; 
            }
            else
            { 
                return VersusModel.CharIdForName(id).ToString(); 
            }
        }

    }
}
