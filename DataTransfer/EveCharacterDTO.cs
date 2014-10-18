using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTransfer
{
    public class EveCharacterDTO
    {
        public int id { get; set; }
        public int? corpID { get; set; }
        public string characterName { get; set; }
        public int? characterEveID { get; set; }
        public string characterApiID { get; set; }
        public string characterApiVcode { get; set; }
        public string rankTitles { get; set; }
        public bool prospect { get; set; }
        public bool formerMember { get; set; }
        public bool currentMember { get; set; }
        public bool blacklist { get; set; }
        public int? altMainCharacterId { get; set; }
        public string comments { get; set; }
        public DateTime lastUpdated { get; set; }

    }
}
