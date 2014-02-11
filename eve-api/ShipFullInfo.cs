using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eve_api
{
    internal class ShipFullInfo
    {
        public int typeID { get; set; }
        public string ShipName { get; set; }
        public string ShipType { get; set; }
        public string description { get; set; }
    }

    /*
     * typeID	ShipName	ShipType	description	difficulty
21097	Goru's Shuttle	Shuttle	Goru Nikainen's Shuttle.	0
     */
}
