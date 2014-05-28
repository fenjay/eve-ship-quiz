using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;

namespace eve_api
{
    public static class eve_api
    {
        public static string GetShipType(string shipName)
        {
            return GetShipFullInfo(shipName).ShipType;
        }

        public static int GetShipId(string shipName)
        {
            return GetShipFullInfo(shipName).typeID;
        }

        public static string GetShipDescription(string shipName)
        {
            return GetShipFullInfo(shipName).description;
        }

        public static List<String> GetRandomShip(int nbrShips, List<int> alreadyAnswered, int quizLevel)
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

        private static string GetConnectionString()
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
        private static List<string> GetRandomFromShipTypeView(int nbrShips, string column, int quizLevel, List<string> excludeNames, List<int> excludeIds, List<string> excludeTypes)
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
                sql.Append("select distinct " + column + " from dbo.ShipTypesView where difficulty > 0 and difficulty <= " + quizLevel.ToString());
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


        public static List<String> GetRandomShipType(int nbrShipTypes, int quizLevel)
        {
            return GetRandomFromShipTypeView(nbrShipTypes, "ShipType", quizLevel, null, null,null);
        }

        public static List<String> GetRandomShipType(int nbrShipTypes, int quizLevel, List<string> excludeNames, List<int> excludeIds, List<string> excludeTypes)
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


    }

}
