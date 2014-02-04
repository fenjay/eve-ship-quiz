using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace eve_api
{
    public static class eve_api
    {

        public static string GetShipType(string shipName)
        {
            var ship = string.Empty;

            var sqlConn = new SqlConnection("Server=localhost;Database=CCP_Data;User Id=APIGetterUser;Password=password1;");
            var sqlCmd = new SqlCommand();
            try
            {

                sqlConn.Open();
                var sql = "select groupName as shipType from invTypes t inner join invGroups g on g.groupID = t.groupID inner join chrRaces race on race.raceID = t.raceID where g.categoryID = 6 and t.published = 1 and typeName = '" + shipName + "'";
                sqlCmd.CommandText = sql;

                sqlCmd.Connection = sqlConn;
                ship = sqlCmd.ExecuteScalar().ToString();
               

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

        public static List<String> GetRandomShip(int nbrShips)
        {
            return GetRandomFromShipTypeView(nbrShips, "ShipName");
        }

        private static List<string> GetRandomFromShipTypeView(int nbrShips, string column)
        {
            var ship = new List<String>();

            var sqlConn = new SqlConnection("Server=localhost;Database=CCP_Data;User Id=APIGetterUser;Password=password1;");
            var sqlCmd = new SqlCommand();
            try
            {
                sqlConn.Open();
                var sqlCount = "select count(distinct " + column + ") from dbo.ShipTypesView";
                
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

                        var sql = "select distinct " + column  + " from dbo.ShipTypesView";
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

        public static List<String> GetRandomShipType(int nbrShipTypes)
        {
            return GetRandomFromShipTypeView(nbrShipTypes, "ShipType");
        }

        public static List<String> GetRandomShipType(int nbrShipTypes, string excludeType)
        {
            return GetRandomFromShipTypeView(nbrShipTypes, "ShipType", excludeType);
        }

        private static List<string> GetRandomFromShipTypeView(int nbrTypes, string column, string exclude)
        {
            var startingList = GetRandomFromShipTypeView(nbrTypes + 1, "ShipType");
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

        //public static List<String> GetAllShipTypes()
        //{

        //}
        

    }

}
