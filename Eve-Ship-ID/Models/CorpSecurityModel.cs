using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataTransfer;

namespace Eve_Ship_ID.Models
{
    public class CorpSecurityModel
    {

        public Dictionary<int, EveCharacterDTO> corpRoster { get; private set; }
        public string errorMessage { get; private set; }
        public bool ValidCorpId { get; set; }  //later, add a method to query the DB for validity
        public EveCorpDTO corpInfo { get; private set; }
        public string CorpImgURL {get;set;}
        public string AllianceURL { get; set; }
        public const string IMG_SERVER_URL = "http://image.eveonline.com/";

        public bool PopulateCorpRoster(int corpId)
        {
            try
            {
                corpRoster = eve_api.eve_corp_security_api.GetCorpRoster(corpId);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public void GetCorpInfo(int corpId)
        {
            corpInfo = eve_api.eve_corp_security_api.GetCorpInfo(corpId);
            CorpImgURL = IMG_SERVER_URL + "Corporation/" + corpInfo.CorpID.ToString() + "_128.png" ;
            AllianceURL = IMG_SERVER_URL + "Alliance/" + corpInfo.AllianceID.ToString() + "_128.png";
        }

        public void RefreshCorpRoster(int corpId)
        {
            eve_api.eve_corp_security_api.RefreshCorpRosterFromAPI(corpId);
            errorMessage = "Corp refreshed from API at " + DateTime.Now;
        }

    }
}