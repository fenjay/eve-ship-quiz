using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eve_api;

namespace Eve_Ship_ID.Models
{
    public class VersusModel
    {

        public static int CharIdForName(string characterName)
        {
            return eve_api.eve_api.CharacterIdForName(characterName);
        }
    }
}