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
using Newtonsoft.Json;

namespace eve_api
{
    public static class eve_api
    {

        private const string CHARACTERIDURL = "https://api.eveonline.com/eve/CharacterID.xml.aspx";  //names= comma separated
        private const string ZKBBASEURL = "https://zkillboard.com/api";
        private const string ZKBLOSSESSUFFIX = "/losses/characterID/";
        private const string ZKBKILLSSUFFIX = "/kills/characterID/";

        public enum QuizLevel
        {
            NORMAL = 0,
            CAPS = 1,
            PIRATE = 2,
            CAPSPIRATE=3
        }

        public static string GetShipType(string shipName)
        {
            return GetShipFullInfo(shipName).ShipType;
        }

        public static int GetShipId(string shipName)
        {
            return GetShipFullInfo(shipName).typeID;
        }


        public static string GetShipNameCSV(string shipNamesCSV)
        {
            var shipIdList = new StringBuilder();
            var shipIdArray = shipNamesCSV.Split(new char[] { ',' });
            var shipId = 0;

            //make sure they are integers. no injecty my sql!
            foreach (var s in shipIdArray)
            {
                if (Int32.TryParse(s, out shipId))
                {
                    shipIdList.Append(shipId.ToString());
                    shipIdList.Append(",");
                }
            }

            if (shipIdList.Length > 0)
            {
                shipIdList.Remove(shipIdList.Length - 1, 1); //snip trailing comma
            }

            var sqlBuilder = new StringBuilder("select typeID, ShipName from ShipTypesView where typeID in (");
            sqlBuilder.Append(shipIdList.ToString());
            sqlBuilder.Append(")");

            var sqlConn = new SqlConnection(GetConnectionString());
            var sqlCmd = new SqlCommand();
            //var shipValueList = new List<KeyValuePair<string,string>>();
            var shipValueList = new Dictionary<string, string>();
            var shipCSV = new StringBuilder();

            try
            {
                sqlConn.Open();
                sqlCmd.CommandText = sqlBuilder.ToString();
                sqlCmd.Connection = sqlConn;
                var shipReader = sqlCmd.ExecuteReader();

                while (shipReader.Read())
                {
                    shipValueList.Add(shipReader.GetInt32(0).ToString(), shipReader.GetString(1));
                }
                shipReader.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }

            var shipName = string.Empty;
            foreach (var s in shipIdArray)
            {
                shipValueList.TryGetValue(s, out shipName);
                shipCSV.Append(shipName);
                shipCSV.Append(",");
            }

            if (shipCSV.Length > 0)
            {
                shipCSV.Remove(shipCSV.Length - 1, 1); //snip trailing comma
            }

            return shipCSV.ToString();

        }


        public static string GetShipDescription(string shipName)
        {
            return GetShipFullInfo(shipName).description;
        }

        public static List<String> GetRandomShip(int nbrShips, List<int> alreadyAnswered, QuizLevel quizLevel)
        {
            return GetRandomFromShipTypeView(nbrShips, "ShipName", quizLevel, null, alreadyAnswered, null);
        }

        private static ShipFullInfo GetShipFullInfo(string shipName)
        {

            var ship = new ShipFullInfo();


            ////TODO: if implementing tests, add "Goru's Shuttle" to test parameter scrubbing
            //if (shipName.Contains("'"))
            //{
            //    shipName.Replace("'", "''");
            //}

            var sqlConn = new SqlConnection(GetConnectionString());
            var sqlCmd = new SqlCommand();
            try
            {
                sqlConn.Open();
                var shipNameParam = new SqlParameter("ShipName", shipName);
                var sql = "select typeID, ShipName, ShipType, description from ShipTypesView where ShipName = @ShipName";
                sqlCmd.Parameters.Add(shipNameParam);
                sqlCmd.CommandText = sql;
                sqlCmd.Connection = sqlConn;
                var shipReader = sqlCmd.ExecuteReader();

                if (shipReader.Read())
                {
                    ship.typeID = Convert.ToInt32(shipReader.GetValue(0));
                    ship.ShipName = shipReader.GetValue(1).ToString();
                    ship.ShipType = shipReader.GetValue(2).ToString();
                    ship.description = shipReader.GetValue(3).ToString();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }

            return ship;

        }

        internal static string GetConnectionString()
        {
            var settingsReader = new AppSettingsReader();
            var sqlConnString = settingsReader.GetValue("ConnectionInfo", typeof(System.String)).ToString();
            return sqlConnString;
        }

        /// <summary>
        /// Get an array of a given number of rows with a given column from the Ship Types View
        /// </summary>
        /// <param name="nbrShips"></param>
        /// <param name="column"></param>
        /// <param name="quizLevel"></param>
        /// <param name="excludeNames"></param>
        /// <param name="excludeIds"></param>
        /// <returns></returns>
        private static List<string> GetRandomFromShipTypeView(int nbrShips, string column, QuizLevel quizLevel, List<string> excludeNames, List<int> excludeIds, List<string> excludeTypes)
        {
            var ship = new List<String>();

            var sqlConn = new SqlConnection(GetConnectionString());
            var sqlCmd = new SqlCommand();
            try
            {
                sqlConn.Open();

                var rand = new Random();
                var shipIndexesToPick = new List<int>();

                var sql = new StringBuilder();
                sql.Append("select distinct " + column + " from dbo.ShipTypesView where " + QuizLevelToSQL(quizLevel));
                sql.Append(AppendClause<string>(excludeNames,"ShipName"));
                sql.Append(AppendClause<int>(excludeIds, "typeID"));
                sql.Append(AppendClause<string>(excludeTypes, "ShipType"));

                System.Diagnostics.Debug.Print(sql.ToString());
                sqlCmd.CommandText = sql.ToString();

                sqlCmd.Connection = sqlConn;
                var shipReader = sqlCmd.ExecuteReader();

                var AllShipTypes = new List<string>();

                while (shipReader.Read())
                {
                    AllShipTypes.Add(shipReader.GetString(0));
                }
                shipReader.Close();

                var nextNbr1 = 0;
                for (var k = 0; k < nbrShips; k++)
                {
                    nextNbr1 = rand.Next(1, AllShipTypes.Count);
                    if (!shipIndexesToPick.Contains(nextNbr1))
                    {
                        shipIndexesToPick.Add(nextNbr1);
                    }
                    else
                    {
                        k--;
                    }
                }

                foreach (var j in shipIndexesToPick)
                {
                    ship.Add(AllShipTypes[j]);
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }
            return ship;
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


        public static List<String> GetRandomShipType(int nbrShipTypes, QuizLevel quizLevel)
        {
            return GetRandomFromShipTypeView(nbrShipTypes, "ShipType", quizLevel, null, null,null);
        }

        public static List<String> GetRandomShipType(int nbrShipTypes, QuizLevel quizLevel, List<string> excludeNames, List<int> excludeIds, List<string> excludeTypes)
        {
            return GetRandomFromShipTypeView(nbrShipTypes, "ShipType", quizLevel, excludeNames, excludeIds, excludeTypes);
        }


        /// <summary>
        /// Note: will not append "where", only "and"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="excludeList"></param>
        /// <param name="column"></param>
        /// <returns>SQL "and" clause</returns>
        private static string AppendClause<T>(List<T> excludeList, string column)
        {
            var sb = new StringBuilder();

            if (excludeList != null && excludeList.Count > 0)
            {
                sb.Append(" and ");
                sb.Append(column);
                sb.Append(" not in (");
                
                for (var s = 0; s < excludeList.Count; s++)
                {
                    if (typeof(T) == typeof(string)) { sb.Append("'"); }
                    sb.Append(excludeList[s].ToString());
                    if (typeof(T) == typeof(string)) { sb.Append("'"); }
                    if (s < excludeList.Count-1) { sb.Append(","); }
                }
                sb.Append(")");
            }
            return sb.ToString();
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
            var charIdUrl = CHARACTERIDURL + "?names=" + characterName;
            var ApiXml = GetApiXml(charIdUrl);
            var charId = 0;

            var nav = GetAPIXmlToResultNode(ApiXml);
            if (nav.Name == "rowset")
            {
                nav.MoveToFirstChild();
            }
            if (nav.Name == "row")
            {
                Int32.TryParse(nav.GetAttribute("characterID", string.Empty), out charId);
            }
            return charId;
        }

        public static string GetKillboardResults(int characterId)
        {
            var startTime = new DateTime(2015,1,1,0,0,0); //start with YTD
            //if we leave out loss #, we just need kills. losses will include much "non-versus" data.
            var retVal = string.Empty;
            var zkbUrl = MakeZKBURL(characterId, ZKBQueryType.Kills, true, startTime); // inline this after debugging
            System.Diagnostics.Debug.Print(zkbUrl);
            retVal = GetApiJson(zkbUrl);
            return retVal;
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
        private static string MakeZKBURL(int characterID, ZKBQueryType queryType, bool noItems, DateTime? startDate)
        {
            var sb = new StringBuilder();
            sb.Append(ZKBBASEURL);
            sb.Append(queryType == ZKBQueryType.Kills ? ZKBKILLSSUFFIX : ZKBLOSSESSUFFIX);
            sb.Append(characterID.ToString());
            sb.Append("/");
            if (noItems) { sb.Append("no-items/"); }
            if (startDate != null) { 
                sb.Append("startTime/"); 
                sb.Append(String.Format("{0:yyyyMMddHHmm}", startDate));
            }

            sb.Append("/");
            return sb.ToString();
            
        }

    }

}
