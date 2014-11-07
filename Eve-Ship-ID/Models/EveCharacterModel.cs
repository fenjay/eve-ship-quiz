using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataTransfer;

namespace Eve_Ship_ID.Models
{
    public class EveCharacterModel
    {
        public EveCharacterDTO EveCharacter {get;set;}
        public string MainCharacterName { get; set; }
        public string ImgURL {get;set;}
        public const string IMG_SERVER_URL = "http://image.eveonline.com/";

        public bool RetrieveCharacter(int EveLocalCharacterId)
        {
            //if character not found, return false

            EveCharacter = eve_api.eve_corp_security_api.GetCharacterFromLocal(EveLocalCharacterId);
            if (EveCharacter.altMainCharacterId != 0)
            {
                MainCharacterName = eve_api.eve_corp_security_api.GetCharacterFromLocal(EveCharacter.altMainCharacterId).characterName;
            }

            ImgURL = IMG_SERVER_URL + "Character/" + EveCharacter.characterEveID.ToString() + "_128.jpg" ;

            return true;
        }

        public bool RefreshCharacterFromAPI(int characterId)
        {

            return true;
        }


        public bool SaveCharacter()
        {
            eve_api.eve_corp_security_api.SaveCharacter(this.EveCharacter);
            return true;
        }
    
    }

}