using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;

using System.Net;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace eve_api
{
    public static class eve_api
    {

        private static IEveDataAccess GetDAL()
        {
            return new SQLDataAccess();
        }

        public enum QuizLevel
        {
            NORMAL = 0,
            CAPS = 1,
            PIRATE = 2,
            CAPSPIRATE = 3
        }



        private const int ZKBMAXRETURN = 200;
        private const string CHARACTERIDURL = "https://api.eveonline.com/eve/CharacterID.xml.aspx";  //names= comma separated
        private const string ZKBBASEURL = "https://zkillboard.com/api";
        private const string ZKBLOSSESSUFFIX = "/losses/characterID/";
        private const string ZKBKILLSSUFFIX = "/kills/characterID/";


        public static string GetShipType(string shipName)
        {
            var dal = GetDAL();
            return dal.GetShipFullInfo(shipName).ShipType;
        }

        public static int GetShipId(string shipName)
        {
            var dal = GetDAL();
            return dal.GetShipFullInfo(shipName).typeID;
        }


       


        public static string GetShipDescription(string shipName)
        {
            var dal = GetDAL();
            return dal.GetShipFullInfo(shipName).description;
        }

        public static List<String> GetRandomShip(int nbrShips, List<int> alreadyAnswered, QuizLevel quizLevel)
        {
            var dal = GetDAL();
            var ql = QuizLevelToList(quizLevel);
            return dal.GetRandomFromShipTypeView(nbrShips, "ShipName", ql, null, alreadyAnswered, null);
        }

       
      

       
        //private static List<string> GetRandomFromShipTypeView(int nbrTypes, string column, string exclude, int quizLevel)
        //{
        //    var startingList = GetRandomFromShipTypeView(nbrTypes + 1, "ShipType", quizLevel);
        //    foreach (var item in startingList)
        //    {
        //        if (item == exclude)
        //        {
        //            startingList.Remove(item);
        //            break;
        //        }
        //    }
        //    if (startingList.Count != nbrTypes)
        //    {
        //        startingList.Remove(startingList[nbrTypes]);
        //    }
        //    return startingList;
        //}

        //public static List<String> GetRandomShipType(int nbrShipTypes, string excludeType, int quizLevel)
        //{
        //    return GetRandomFromShipTypeView(nbrShipTypes, "ShipType", excludeType, quizLevel);
        //}


        public static string GetShipNameCSV(string shipNamesCSV)
        {
            var dal = GetDAL();
            return dal.GetShipNameCSV(shipNamesCSV);
        }

        public static List<String> GetRandomShipType(int nbrShipTypes, QuizLevel quizLevel)
        {
            var dal = GetDAL();
            var ql = QuizLevelToList(quizLevel);
            return dal.GetRandomFromShipTypeView(nbrShipTypes, "ShipType", ql, null, null, null);
        }

        public static List<String> GetRandomShipType(int nbrShipTypes, QuizLevel quizLevel, List<string> excludeNames, List<int> excludeIds, List<string> excludeTypes)
        {
            var dal = GetDAL();
            var ql = QuizLevelToList(quizLevel);
            return dal.GetRandomFromShipTypeView(nbrShipTypes, "ShipType", ql, excludeNames, excludeIds, excludeTypes);
        }

        private static string QuizLevelToSQL(QuizLevel q)
        {
            var sql = string.Empty;

            switch (q)
            {
                case QuizLevel.NORMAL:
                    {
                        sql = "difficulty in (1)";
                        break;
                    }
                case QuizLevel.CAPS:
                    {
                        sql = "difficulty in (1,2)";
                        break;
                    }

                case QuizLevel.PIRATE:
                    {
                        sql = "difficulty in (1,3)";
                        break;
                    }

                case QuizLevel.CAPSPIRATE:
                    {
                        sql = "difficulty in (1,2,3)";
                        break;
                    }
                default:
                    {
                        sql = "difficulty in (1)";
                        break;
                    }
            }

            return sql;
        }


        private static List<int> QuizLevelToList(QuizLevel q)
        {
            var list = new List<int>();

            switch (q)
            {
                case QuizLevel.NORMAL:
                    {
                        list.Add(1);
                        break;
                    }
                case QuizLevel.CAPS:
                    {
                        list.Add(1);
                        list.Add(2);
                        break;
                    }

                case QuizLevel.PIRATE:
                    {
                        list.Add(1);
                        list.Add(3);
                        break;
                    }

                case QuizLevel.CAPSPIRATE:
                    {
                        list.Add(1);
                        list.Add(2);
                        list.Add(3);
                        break;
                    }
                default:
                    {
                        list.Add(1);
                        break;
                    }
            }

            return list;
        }


        private static XmlDocument GetApiXml(string apiUrl)
        {
            try
            {        //disable cache timeout for now - reimplement when local caching is available
                //use http://deanhume.com/Home/BlogPost/object-caching----net-4/37 - .net object caching
                //if (VerifyAPICacheTimeExpired(apiUrl))
                //{
                var wr = WebRequest.Create(apiUrl);
                //wr.Headers.Add(HttpRequestHeader.UserAgent, "fenjaylabs.com/CorpSecurity/0.1");

                var response = wr.GetResponse();
                var xmldoc = new XmlDocument();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    xmldoc.Load(sr);
                    //string x = sr.ReadToEnd();
                }

                //var cachedUntil = GetCachedUntil(xmldoc);
                //SetCacheTime(apiUrl, cachedUntil);
                return xmldoc;
                //}
                //else
                //{
                //    return new XmlDocument();
                //}
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.Print("Web exception " + ((HttpWebResponse)ex.Response).StatusCode.ToString());
                return new XmlDocument();
            }

        }


        private static XPathNavigator GetAPIXmlToResultNode(XmlDocument ApiXml)
        {
            var nav = ApiXml.CreateNavigator();
            nav.MoveToRoot();
            nav.MoveToFirstChild(); //eveapi
            nav.MoveToFirstChild(); //currenttime
            if (nav.Name == "currentTime")
            {
                nav.MoveToNext();
            }
            if (nav.Name == "result")
            {
                nav.MoveToFirstChild();
            }
            return nav;
        }





        //-------------------- Versus section -------------------------------------------------------------------------------

        public static int CharacterIdForName(string characterName)
        {
            var dal = GetDAL();
            var charId = dal.GetEveIDForCharacterFromLocal(characterName);

            System.Diagnostics.Debug.Print("The CharID from local is " + charId.ToString());
            if (charId == 0)
            {
                var charIdUrl = CHARACTERIDURL + "?names=" + characterName;
                var ApiXml = GetApiXml(charIdUrl);
             
                var nav = GetAPIXmlToResultNode(ApiXml);
                if (nav.Name == "rowset")
                {
                    nav.MoveToFirstChild();
                }
                if (nav.Name == "row")
                {
                    Int32.TryParse(nav.GetAttribute("characterID", string.Empty), out charId);
                }
                //TODO: Hey! Let's store the right capitalization since we only have to call once!
                System.Diagnostics.Debug.Print("I got the charID from the API!");
                dal.SaveEveCharacterIDToLocal(charId, characterName);
            }
            return charId;
        }

        public static string GetKillboardResults(int characterId)
        {
            var startTime = new DateTime(2015, 1, 1, 0, 0, 0); //start with YTD
            var zkbKillResults = string.Empty;
            var combinedKillResults = string.Empty;

            //if we leave out loss #, we just need kills. losses will include much "non-versus" data.

            //pseudo: get max killID from database. then query zkb for kills after max kill id. if 0 results from db, just do a base query (all kills from startTime)
            //(later, with adjustable date range, get min killId and get kills before it too)
            // return: three possibilities.  1- no local data. get zkb data, save in db, return it. 2 - all local data. no new zkb kills. return local data. 3 - mixed. splice local +zkb data

            //currently assumes that we don't ever need earlier kills. if/when we change from YTD, this will need to be changed

            var dal = GetDAL();

            //get maximum kill id in database, if available
            int? maxLocalKillId = null;
            maxLocalKillId = dal.GetMaxKillIdFromLocal(characterId);  //maybe just get the kills and order by kill id desc?

            //get all other kill data with higher id than locally cached data
            zkbKillResults = GetZKBData(characterId, startTime, maxLocalKillId);

            //if there is any local data, pull it first
            var localResultsStr = new StringBuilder();
            if (maxLocalKillId != null)
            {
                var localResults = dal.GetLocalKillResults(characterId);
                localResultsStr.Append("[");

                for (var i = 0; i < localResults.Count; i++)
                {
                    var s = localResults[i];
                    localResultsStr.Append(s);
                    localResultsStr.Append(i == localResults.Count - 1 ? "]" : ",");
                }
            }

            //after getting local data (so there is no confusion or doubling), save locally the more recent kb data we retrieved earlier (if any)
            var jsonObject = JArray.Parse(zkbKillResults);
            foreach (var j in jsonObject)
            {
                var killID = Int32.Parse(j["killID"].ToString());
                dal.InsertKillJson(characterId, killID, j.ToString());
            }

            //then merge and return zkb data, local data, or both as applicable.
            if (zkbKillResults.Length > 6 && localResultsStr.Length > 6)  //these must be at least 6 (length of "killID") to be valid JSON. This is a shortcut.
            {
                combinedKillResults = JsonSplice(zkbKillResults, localResultsStr.ToString());
            }
            else if (zkbKillResults.Length > 6)
            {
                combinedKillResults = zkbKillResults;
            }
            else if (localResultsStr.Length > 6)
            {
                combinedKillResults = localResultsStr.ToString();
            }
            else
            {
                combinedKillResults = "[]"; //valid, but empty JSON
            }

            return combinedKillResults;
        }
        
        private static string GetZKBData(int characterId, DateTime? startTime, int? maxLocalKillId)
        {  //consider adding 1 second delay between each page
            var zkbResultsStr = string.Empty;
            var zkbUrl = MakeZKBURL(characterId, ZKBQueryType.Kills, true, startTime, 0, maxLocalKillId);
            System.Diagnostics.Debug.Print(zkbUrl);
            zkbResultsStr = GetApiJson(zkbUrl);
            var zkbResultsJson = JArray.Parse(zkbResultsStr);

            int currentPage = 1;
            while (zkbResultsJson.Count == ZKBMAXRETURN)
            {
                currentPage++;
                var zkbPaged = MakeZKBURL(characterId, ZKBQueryType.Kills, true, startTime, currentPage, maxLocalKillId);
                System.Diagnostics.Debug.Print(zkbPaged);

                var zkbResultsStrPaged = string.Empty;
                zkbResultsStrPaged = GetApiJson(zkbPaged);

                zkbResultsStr = JsonSplice(zkbResultsStr, zkbResultsStrPaged); // add on the next (up to) 200 kills

                zkbResultsJson = JArray.Parse(zkbResultsStrPaged); //see if this is also at max returned kills for another go-round
            }

            return zkbResultsStr;
        }

        private static string JsonSplice(string Json1, string Json2)
        {
            return String.Concat(Json1.Remove(Json1.Length - 1, 1),",", Json2.Remove(0, 1)); //cut off the trailing and leading ][ characters
        }



        private static string GetApiJson(string apiUrl)
        {
            try
            {
                var wr = (HttpWebRequest)HttpWebRequest.Create(apiUrl);
                wr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                wr.UserAgent = "fenjaylabs.com/Versus/0.1 Maintainer fenjay@fenjaylabs.com";
                //wr.Headers.Add(HttpRequestHeader.UserAgent, "fenjaylabs.com/Versus/0.1");

                var response = wr.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return (sr.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.Print("Web exception " + ((HttpWebResponse)ex.Response).StatusCode.ToString());
                return string.Empty;
            }
        }


        private enum ZKBQueryType
        {
            Kills,
            Losses
        }
        private static string MakeZKBURL(int characterID, ZKBQueryType queryType, bool noItems, DateTime? startDate, int pageNbr, int? afterKillId)
        {
            var sb = new StringBuilder();
            sb.Append(ZKBBASEURL);
            sb.Append(queryType == ZKBQueryType.Kills ? ZKBKILLSSUFFIX : ZKBLOSSESSUFFIX);
            sb.Append(characterID.ToString());
            
            if (noItems) { sb.Append("/no-items"); }
            
            if (startDate != null) { 
                sb.Append("/startTime/"); 
                sb.Append(String.Format("{0:yyyyMMddHHmm}", startDate));
            }
            
            if(pageNbr > 1)
            {
                sb.Append("/page/");
                sb.Append(pageNbr.ToString());
            }

            if(afterKillId != null)
            {
                sb.Append("/afterKillID/");
                sb.Append(afterKillId.ToString());
            }

            sb.Append("/");
            return sb.ToString();
        
        }



    }

}
