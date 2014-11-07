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
            //get corp list
            return View();
        }

        public ActionResult RefreshCorporation(string id)
        {
            var data = new CorpSecurityModel();

              var idParsed = 0;

            //add bourbon and blasters for testing
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
                data.GetCorpInfo(idParsed);
                if (data.corpInfo.CorpName != string.Empty)
                {
                    data.ValidCorpId = true;
                    data.PopulateCorpRoster(idParsed);
                }
                else
                {
                    data.ValidCorpId = false;
                }

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

            return View(data);
        }

        [HttpPost]
        public ActionResult EveCharacterEdit(EveCharacterModel updatedCharacter)
        {
            System.Diagnostics.Debug.Write(updatedCharacter.EveCharacter.comments);
            
            updatedCharacter.SaveCharacter();

            return new RedirectResult("../CorporationView/" + updatedCharacter.EveCharacter.corpID.ToString());
        }

        public string IsApiExpired(string id)
        {
            if (id == null) { return "REQUIRED: characterId,api,vCode"; }

            var pm = id.Split(new string[] { "," }, StringSplitOptions.None);

            if ((pm.Length == 3) && (pm[0] != string.Empty) && (pm[1] != string.Empty) && (pm[2] != string.Empty))
            {
                var characterid = pm[0];
                var api = pm[1];
                var vcode = pm[2];


                if (eve_api.eve_corp_security_api.IsCharacterAPIExpired(characterid, api, vcode))
                {
                    return "YES";
                }
                else
                {
                    return "NO";
                }
            }

            return "INVALID";
        }

    }
}
