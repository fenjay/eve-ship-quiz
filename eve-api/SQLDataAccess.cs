using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;

namespace eve_api
{
    internal class SQLDataAccess : IEveDataAccess
    {
        public int GetEveIDForCharacterFromLocal(string characterName)
        {
            var charId = 0;
            try
            {
                var p = new SqlParameter("CharacterName", characterName.ToLower());
                var sql = "select CharacterId from VS_FJL_CharacterIDName where LOWER(CharacterName)=@CharacterName";

                SqlConnection conn;
                SqlCommand cmd;
                GetDataAccessObjects(out conn, out cmd);

                using (conn)
                {
                    cmd.Parameters.Add(p);
                    cmd.CommandText = sql;
                    var sqlRetObj = cmd.ExecuteScalar();
                    charId = sqlRetObj == null ? 0 : (int)sqlRetObj;
                }
            }
            catch(Exception ex)
            {
                Debug.Print(ex.Message);
            }
            return charId;
        }

        public string GetKillboardResultsFromLocal(int characterId)
        {
            throw new NotImplementedException();
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
        public List<string> GetRandomFromShipTypeView(int nbrShips, string column, List<int> quizLevel, List<string> excludeNames, List<int> excludeIds, List<string> excludeTypes)
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
                sql.Append(AppendClause<string>(excludeNames, "ShipName"));
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

        private string QuizLevelToSQL(List<int> quizLevels)
        {
            var sb = new StringBuilder();
            sb.Append("difficulty in (");
            foreach(var l in quizLevels)
            {
                sb.Append(l.ToString());
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1); //trailing comma
            sb.Append(")");
            return sb.ToString();
        }

        public ShipFullInfo GetShipFullInfo(string shipName)
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

        public string GetShipNameCSV(string shipNamesCSV)
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

        public bool SaveKillboardResultsToLocal(string kbJson)
        {
            throw new NotImplementedException();
        }

        
        // -------------^ interface ----------v private

        internal string GetConnectionString()
        {
            var settingsReader = new AppSettingsReader();
            var sqlConnString = settingsReader.GetValue("ConnectionInfo", typeof(System.String)).ToString();
            return sqlConnString;
        }

        internal void GetDataAccessObjects(out SqlConnection conn, out SqlCommand cmd)
        {
            conn = new SqlConnection(GetConnectionString());
            conn.Open();
            cmd = new SqlCommand();
            cmd.Connection = conn;
        }

        /// <summary>
        /// Note: will not append "where", only "and"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="excludeList"></param>
        /// <param name="column"></param>
        /// <returns>SQL "and" clause</returns>
        private string AppendClause<T>(List<T> excludeList, string column)
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
                    if (s < excludeList.Count - 1) { sb.Append(","); }
                }
                sb.Append(")");
            }
            return sb.ToString();
        }



        public bool SaveEveCharacterIDToLocal(int characterId, string characterName)
        {
            var id = new SqlParameter("charId", characterId);
            var name = new SqlParameter("charName", characterName);
            var sql = "insert into VS_FJL_CharacterIDName (CharacterId, CharacterName) values (@charId,@charName)";
            
            SqlConnection conn;
            SqlCommand cmd;
            GetDataAccessObjects(out conn, out cmd);
            
            try
            {
             
                using (conn)
                {
                    cmd.Parameters.Add(id);
                    cmd.Parameters.Add(name);
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                Debug.Print("I saved the following: id:" + characterId.ToString() + " name:" + characterName);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                return false;
            }
            finally
            {
                conn.Close();
            }
            return true;
            
        }

        public int? GetMaxKillIdFromLocal(int characterId)
        {
            int? maxKillId = null;


            SqlConnection conn;
            SqlCommand cmd;
            GetDataAccessObjects(out conn, out cmd);

            try
            {
                var charId = new SqlParameter("charId", characterId);

                using(conn)
                {
                    cmd.CommandText = "select max(KillID) from VS_FJL_KillDataJson where AttackingCharacterID = @charId";
                    cmd.Parameters.Add(charId);
                    var oKillId = cmd.ExecuteScalar();
                    if (!(oKillId is System.DBNull))
                    {
                        maxKillId = Int32.Parse(oKillId.ToString());
                        //else, leave it null
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            return maxKillId;
        }

        public List<string> GetLocalKillResults(int characterId)
        {
            SqlConnection conn;
            SqlCommand cmd;
            GetDataAccessObjects(out conn, out cmd);
            var kills = new List<string>();

            
            try
            {
                var charId = new SqlParameter("charId", characterId);
                cmd.Parameters.Add(charId);
                cmd.CommandText = "select KillJSON from VS_FJL_KillDataJson where AttackingCharacterID = @charId";

                var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    kills.Add(reader.GetString(0));
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            return kills;
        }

        public void InsertKillJson(int characterId, int killID, string killJson)
        {
            SqlConnection conn;
            SqlCommand cmd;
            GetDataAccessObjects(out conn, out cmd);
               
            try
            {
                SqlParameter[] inputparams = {new SqlParameter("killJson", killJson),new SqlParameter("killID", killID),new SqlParameter("characterId", characterId)};

                //var paramKillJson = new SqlParameter("killJson", killJson);
                //var paramKillID = new SqlParameter("killID", killID);
                //var paramCharacterId = new SqlParameter("characterId", characterId);

                cmd.Parameters.AddRange(inputparams);
                cmd.CommandText = "insert into VS_FJL_KillDataJson (KillID, KillJSON, AttackingCharacterID) values (@killID, @killJson, @characterId)";
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }

    }
}
