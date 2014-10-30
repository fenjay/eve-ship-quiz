using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTransfer
{
    public class EveCorpDTO
    {
        public int CorpID { get; set; }
        public string CorpName { get;  set; }
        public string CorpLogoUrl { get;  set; }
        public string AllianceName { get;  set; }
        public int AllianceID { get;  set; }
        public string AllianceLogoUrl { get;  set; }
    }
}
