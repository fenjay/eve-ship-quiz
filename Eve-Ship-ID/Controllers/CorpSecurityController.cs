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

            data.PopulateCorpRoster(98340372);         

            return View(data);
        }

        public ActionResult RefreshCorporation(string id)
        {
            var data = new CorpSecurityModel();

              var idParsed = 0;

              if (Int32.TryParse(id, out idParsed) && idParsed == 98340372) //or another valid value
              {
                  data.RefreshCorpRoster(idParsed);
              }

              return Redirect("../CorporationView/" + id);
            
        }

        public ActionResult CorporationView(string id) //(int CorpId)  //maybe change parameter to a or hidden param later
        {
            var data = new CorpSecurityModel();
            var idParsed = 0;

            if (Int32.TryParse(id,out idParsed) && idParsed == 98340372) //or another valid value
            {
                data.ValidCorpId = true;

                //if (this.HttpContext.Request.QueryString["refresh"] == "1")
                //{
                //    data.RefreshCorpRoster(idParsed);
                //}


                data.PopulateCorpRoster(idParsed);  //(id);
            }
            else
            {
                data.ValidCorpId = false;
            }

            return View(data);
        }

        public ActionResult EveCharacterEdit(string id)
        {
            var data = new EveCharacterModel();
            var idParsed = 0;

            if (Int32.TryParse(id, out idParsed)) //or another valid value
            data.RetrieveCharacter(idParsed);

            data.EveCharacter.characterEveID = idParsed;
            
            return View(data);
        }

    }
}
