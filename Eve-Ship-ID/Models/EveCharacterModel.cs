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
        public string ImgURL {get;set;}
        public const string IMG_SERVER_URL = "http://image.eveonline.com/";

        public bool RetrieveCharacter(int EveCharacterId)
        {
            //if character not found, return false

            EveCharacter = new EveCharacterDTO();

            EveCharacter.characterName = "Test Name";
            EveCharacter.characterApiID = " test api";
            EveCharacter.characterApiVcode = "test vcode";
            EveCharacter.rankTitles = "test titles";

            ImgURL = IMG_SERVER_URL + "Character/" + EveCharacterId.ToString() + "_128.jpg" ;

            return true;
        }

        public bool SaveCharacter()
        {
            return true;
        }
    
    }

}