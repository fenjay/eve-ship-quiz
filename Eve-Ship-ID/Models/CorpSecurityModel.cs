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

        public void RefreshCorpRoster(int corpId)
        {
            eve_api.eve_corp_security_api.RefreshCorpRosterFromAPI(corpId);
            errorMessage = "Corp refreshed from API at " + DateTime.Now;
        }

    }
}