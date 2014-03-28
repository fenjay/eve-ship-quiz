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

        public static List<String> GetRandomShip(int nbrShips, int quizLevel)
        {
            return GetRandomFromShipTypeView(nbrShips, "ShipName", quizLevel);
        }

        private static ShipFullInfo GetShipFullInfo(string shipName)
        {

            var ship = new ShipFullInfo();


            ////TODO: if implementing tests, add "Goru's Shuttle" to test parameter scrubbing
            //if (shipName.Contains("'"))
            //{
            //    shipName.Replace("'", "''");
            //}

            var settingsReader= new AppSettingsReader();
            var sqlConnString = settingsReader.GetValue("ConnectionInfo", typeof(System.String)).ToString();
            var sqlConn = new SqlConnection(sqlConnString);
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

        private static List<string> GetRandomFromShipTypeView(int nbrShips, string column, int quizLevel)
        {
            var ship = new List<String>();

            var sqlConn = new SqlConnection("Server=localhost;Database=CCP_Data;User Id=APIGetterUser;Password=password1;");
            var sqlCmd = new SqlCommand();
            try
            {
                sqlConn.Open();
                var sqlCount = "select count(distinct " + column + ") from dbo.ShipTypesView where difficulty > 0 and difficulty <= " + quizLevel.ToString();
                
                sqlCmd.CommandText = sqlCount;
                sqlCmd.Connection = sqlConn;
                var shipCount = 0;
                var rand = new Random();
                var shipNbrsToPick = new List<int>();


                if (Int32.TryParse(sqlCmd.ExecuteScalar().ToString(), out shipCount))
                {
                    var nextNbr = 0;
                    for (var k = 0; k < nbrShips; k++)
                    {
                        nextNbr = rand.Next(1, shipCount);
                        if (!shipNbrsToPick.Contains(nextNbr))
                        {
                            shipNbrsToPick.Add(nextNbr);
                        }
                        else
                        {
                            k--;
                        }
                    }
                

                
                //if (Int32.TryParse(sqlCmd.ExecuteScalar().ToString(), out shipCount))
                //{
                //    for (var j = 0; j < nbrShips; j++)
                //    {
                        //var shipNbr = rand.Next(1, shipCount);

                        var sql = "select distinct " + column + " from dbo.ShipTypesView where difficulty > 0 and difficulty <= " + quizLevel.ToString();
                        sqlCmd.CommandText = sql;

                        sqlCmd.Connection = sqlConn;
                        var shipReader = sqlCmd.ExecuteReader();

                        var i = 0;
                        while (shipReader.Read())
                        {
                            if (shipNbrsToPick.Contains(i))
                            {
                                ship.Add(shipReader.GetString(0));
                            }
                            i++;

                        }
                        shipReader.Close();
                //    }
                }
                else
                {
                    return new List<string>();
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

        private static List<string> GetRandomFromShipTypeView(int nbrTypes, string column, string exclude, int quizLevel)
        {
            var startingList = GetRandomFromShipTypeView(nbrTypes + 1, "ShipType", quizLevel);
            foreach (var item in startingList)
            {
                if (item == exclude)
                {
                    startingList.Remove(item);
                    break;
                }
            }
            if (startingList.Count != nbrTypes)
            {
                startingList.Remove(startingList[nbrTypes]);
            }
            return startingList;
        }

        public static List<String> GetRandomShipType(int nbrShipTypes, string excludeType, int quizLevel)
        {
            return GetRandomFromShipTypeView(nbrShipTypes, "ShipType", excludeType, quizLevel);
        }

        public static List<String> GetRandomShipType(int nbrShipTypes, int quizLevel)
        {
            return GetRandomFromShipTypeView(nbrShipTypes, "ShipType", quizLevel);
        }

    }

}
