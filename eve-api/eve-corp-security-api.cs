using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using DataTransfer;
using System.Net;
using System.IO;
using System.Xml;

namespace eve_api
{
    public static class eve_corp_security_api
    {

        private const string CORPROSTERURL = "https://api.eveonline.com/corp/MemberTracking.xml.aspx";

        public static Dictionary<int,EveCharacterDTO> GetCorpRoster(int corpId)
        {
            var corpRoster = new Dictionary<int, EveCharacterDTO>();

            var sql = "select CharacterInfoId, CorpID, CharacterId, CharacterName, CharacterApiId, CharacterApiVcode, TitlesRanks, IsProspect, IsFormerMember, IsCurrentMember, IsBlacklisted, AltMainCharacterId, Comments from CS_FJL_CharacterInfo c where c.CorpID = " + corpId.ToString() + " and c.IsCurrentMember=1";

            var sqlConn = new SqlConnection(eve_api.GetConnectionString());
            var sqlCmd = new SqlCommand();
                        
            try
            {
                sqlConn.Open();
                sqlCmd.CommandText = sql;
                sqlCmd.Connection = sqlConn;
                var corpListReader = sqlCmd.ExecuteReader();

                while (corpListReader.Read())
                {
                    var dto = new EveCharacterDTO();
                    dto.id = corpListReader.GetInt32(0);
                    dto.corpID = GetNullableInt(corpListReader, 1);
                    dto.characterEveID = GetNullableInt(corpListReader, 2);
                    dto.characterName = corpListReader.GetString(3);
                    dto.characterApiID = GetNullableString(corpListReader, 4);
                    dto.characterApiVcode = GetNullableString(corpListReader, 5);
                    dto.rankTitles = GetNullableString(corpListReader, 6);
                    //IsProspect, IsFormerMember, IsCurrentMember, IsBlacklisted, AltMainCharacterId, Comments
                    dto.prospect = corpListReader.GetBoolean(7);
                    dto.formerMember = corpListReader.GetBoolean(8);
                    dto.currentMember = corpListReader.GetBoolean(9);
                    dto.blacklist = corpListReader.GetBoolean(10);
                    dto.altMainCharacterId = GetNullableInt(corpListReader, 11);

                    dto.comments = GetNullableString(corpListReader, 12);

                    corpRoster.Add(corpListReader.GetInt32(2),dto);
                }
                corpListReader.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }


            return corpRoster;

            
        }

        public static EveCharacterDTO GetCharacterFromLocal(int eveLocalCharacterid)
        {
            var sql = "select CharacterInfoId, CorpID, CharacterName, CharacterApiId, CharacterApiVcode, TitlesRanks, IsProspect, IsFormerMember, IsCurrentMember, IsBlacklisted, AltMainCharacterId, Comments, CharacterId from CS_FJL_CharacterInfo c where c.CharacterInfoId = " + eveLocalCharacterid.ToString();

            var sqlConn = new SqlConnection(eve_api.GetConnectionString());
            var sqlCmd = new SqlCommand();
            var charRetrieved = new EveCharacterDTO();

            try
            {
                sqlConn.Open();
                sqlCmd.CommandText = sql;
                sqlCmd.Connection = sqlConn;
                var characterReader = sqlCmd.ExecuteReader();
                if (characterReader.Read())
                {
                    charRetrieved.id = characterReader.GetInt32(0);
                    charRetrieved.corpID = GetNullableInt(characterReader, 1);
                    charRetrieved.characterName = characterReader.GetString(2);
                    charRetrieved.characterApiID = GetNullableString(characterReader, 3);
                    charRetrieved.characterApiVcode = GetNullableString(characterReader, 4);
                    charRetrieved.rankTitles = GetNullableString(characterReader, 5);
                    charRetrieved.prospect = characterReader.GetBoolean(6);
                    charRetrieved.formerMember = characterReader.GetBoolean(7);
                    charRetrieved.currentMember = characterReader.GetBoolean(8);
                    charRetrieved.blacklist = characterReader.GetBoolean(9);
                    charRetrieved.altMainCharacterId = GetNullableInt(characterReader, 10);
                    charRetrieved.comments = GetNullableString(characterReader, 11);
                    charRetrieved.characterEveID = GetNullableInt(characterReader, 12);
                }
            }
            finally
            {
                sqlConn.Close();
            }
            return charRetrieved;
        }

        public static List<string> GetCharacterBlacklist()
        {
            return new List<string>();
        }

        public static List<string> GetProspects()
        {
            return new List<string>();
        }

        public static List<string> GetCorpBlacklist()
        {
            return new List<string>();
        }

        public static List<string> GetCorpInfo(int corpId)
        {
            return new List<string>();
        }

        public static List<string> GetWalletTransactions(int characterId)
        {
            return new List<string>();
        }

        public static List<string> GetEveMails(int characterId)
        {
            return new List<string>();
        }

        public static List<string> GetKillMailActivity(int characterId)
        {
            return new List<string>();
        }

        public static List<string> GetCorpHistory(int characterId)
        {
            return new List<string>();
        }

        public static void RefreshCorpRosterFromAPI(int corpId)
        {
            string CorpRosterURL = CORPROSTERURL + GetCorpMasterApiVcode(corpId);
            System.Diagnostics.Debug.Print(CorpRosterURL);
            
            //1. update character listing. 2. update each character
            //don't forget to escape single quotes. thanks Dr La'Vey
            //try catch throw. add error message in controller.
            var corpxml = GetApiXml(CorpRosterURL);

            //copy each record into a DTO
            //foreach dto: if existing, update.
            //             if doesn't exist, create
            //             if in table but not DTO list, mark as former member and clear current member. otherwise update
            var nav = corpxml.CreateNavigator();
            var updateTime = DateTime.Now;
            var updateSet = new List<EveCharacterDTO>();

            nav.MoveToRoot();
            nav.MoveToFirstChild(); //eveapi
            nav.MoveToFirstChild(); //currenttime
            if (nav.Name == "currentTime")
            {
                updateTime = DateTime.Parse(nav.Value);
                nav.MoveToNext(); //result
            }
            if(nav.Name == "result")
            {
                nav.MoveToFirstChild(); //rowset
                if(nav.Name == "rowset")
                {
                    nav.MoveToFirstChild();  //row(s)
                    if(nav.Name == "row")
                    {
                        do
                        {
                            var corpChar = new EveCharacterDTO();
                            corpChar.characterName = nav.GetAttribute("name", string.Empty);
                            corpChar.rankTitles = nav.GetAttribute("title", string.Empty);
                            corpChar.currentMember = true;
                            corpChar.corpID = corpId;
                            corpChar.characterEveID = Int32.Parse(nav.GetAttribute("characterID", string.Empty));

                            updateSet.Add(corpChar);

                        } while (nav.MoveToNext());
                    }
                }
            }
            foreach (var corpEntry in updateSet)
            {
                System.Diagnostics.Debug.Print(corpEntry.characterName + "|" + corpEntry.characterEveID);
            }
            //update into db.
            UpsertCorpChars(updateSet, corpId, true);
            

        }

        private static void UpsertCorpChars(List<EveCharacterDTO> theCharacters, int corpId, bool fromApiOnly)
        {
            var existingCorp = GetCorpRoster(corpId);
            foreach (var character in theCharacters)
            {
                int charId = character.characterEveID;
                if (charId == 0)
                {
                    //try to get the character id from the API. if success, continue. If failure, skip this character
                }
                
                UpsertCorpChar(character, existingCorp.ContainsKey(charId) ? existingCorp[charId] : null,fromApiOnly);
                
            }


        }

        private static void UpsertCorpChar(EveCharacterDTO theNewCharacter, EveCharacterDTO theOldCharacter, bool fromApiOnly)
        {
            var sql = new StringBuilder();

            if (theOldCharacter != null) //update
            {
                var updateAvailable = false;
                sql.Append("update CS_FJL_CharacterInfo set ");
                
                //if updating from API, do not set the "extra" info such as comments
                //if not (i.e. updating from combined) then update all

                if (theNewCharacter.corpID != theOldCharacter.corpID) { 
                    sql.Append("CorpId=");
                    sql.Append(theNewCharacter.corpID.ToString());
                    sql.Append(",");
                    updateAvailable = true;
                }

                //characterId will presumably never update

             if (theNewCharacter.rankTitles != theOldCharacter.rankTitles)
                {
                    sql.Append("TitlesRanks=");
                    sql.Append(theNewCharacter.rankTitles);
                    sql.Append(",");
                    updateAvailable = true;
                }

             if (!fromApiOnly)
             {

                 if (theNewCharacter.characterApiID != theOldCharacter.characterApiID)
                 {
                     sql.Append("CharacterApiId=");
                     sql.Append(theNewCharacter.characterApiID);
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.characterApiVcode != theOldCharacter.characterApiVcode)
                 {
                     sql.Append("CharacterApiVcode=");
                     sql.Append(theNewCharacter.characterApiVcode);
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.prospect != theOldCharacter.prospect)
                 {
                     sql.Append("IsProspect=");
                     sql.Append(theNewCharacter.prospect ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.formerMember != theOldCharacter.formerMember)
                 {
                     sql.Append("IsFormerMember=");
                     sql.Append(theNewCharacter.formerMember ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.currentMember != theOldCharacter.currentMember)
                 {
                     sql.Append("IsCurrentMember=");
                     sql.Append(theNewCharacter.currentMember ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.blacklist != theOldCharacter.blacklist)
                 {
                     sql.Append("IsBlacklisted=");
                     sql.Append(theNewCharacter.blacklist ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.altMainCharacterId != theOldCharacter.altMainCharacterId)
                 {
                     sql.Append("AltMainCharacterId=");
                     sql.Append(theNewCharacter.altMainCharacterId.ToString());
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.comments != theOldCharacter.comments)
                 {
                     sql.Append("Comments=");
                     sql.Append(theNewCharacter.comments);
                     sql.Append(",");
                     updateAvailable = true;
                 }
             }
                
                sql.Remove(sql.Length - 1, 1); //remove comma
                sql.Append(" where CharacterId = " + theNewCharacter.id);
                
                if (!updateAvailable) { return; } //bail out, no update to this one
            }
            else //insert
            {
                sql.Append("insert into CS_FJL_CharacterInfo "); 
                sql.Append("(CorpID,CharacterId,CharacterName,CharacterApiId,CharacterApiVcode,TitlesRanks,IsProspect,IsFormerMember,IsCurrentMember,IsBlacklisted,AltMainCharacterId,Comments)");
                sql.Append(" values (");
                sql.Append(theNewCharacter.corpID.ToString());
                sql.Append(",");
                sql.Append(theNewCharacter.characterEveID.ToString());
                sql.Append(",'");
                sql.Append(theNewCharacter.characterName.Replace("'","''")); 
                sql.Append("','");
                sql.Append(theNewCharacter.characterApiID); 
                sql.Append("','");
                sql.Append(theNewCharacter.characterApiVcode); 
                sql.Append("','");
                sql.Append(theNewCharacter.rankTitles.Replace("'","''")); 
                sql.Append("',");
                sql.Append(theNewCharacter.prospect ? "1": "0"); 
                sql.Append(",");
                sql.Append(theNewCharacter.formerMember ? "1" : "0"); 
                sql.Append(",");
                sql.Append(theNewCharacter.currentMember ? "1" : "0"); 
                sql.Append(",");
                sql.Append(theNewCharacter.blacklist ? "1" : "0"); 
                sql.Append(",");
                sql.Append(theNewCharacter.altMainCharacterId.ToString()); 
                sql.Append(",'");
                sql.Append(theNewCharacter.comments.Replace("'","''")); 
                sql.Append("')");
                    	
            }

            var sqlConn = new SqlConnection(eve_api.GetConnectionString());
            var sqlCmd = new SqlCommand();
                        
            try
            {
                sqlConn.Open();
                sqlCmd.CommandText = sql.ToString();
                sqlCmd.Connection = sqlConn;
                var corpListReader = sqlCmd.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }


        }

        private static string GetCorpMasterApiVcode(int corpId)
        {
            var sql = "select CorpMasterApiId, CorpMasterApiVcode from CS_FJL_UserCorpInfo where CorpID = " + corpId;
            var masterApi = string.Empty;
            var masterVcode = string.Empty;


            var sqlConn = new SqlConnection(eve_api.GetConnectionString());
            var sqlCmd = new SqlCommand();

            try
            {
                sqlConn.Open();
                sqlCmd.CommandText = sql;
                sqlCmd.Connection = sqlConn;
                var corpReader = sqlCmd.ExecuteReader();
                if (corpReader.Read())
                {
                    masterApi = corpReader.GetString(0);
                    masterVcode = corpReader.GetString(1);
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

            return "?keyID=" + masterApi + "&vcode=" + masterVcode;

        }

        public static bool RefreshCharacterFromAPI(int characterId)
        {
            return true;
        }




        //-------------------- Utility Methods ----------------------------//

        private static string GetNullableString(SqlDataReader d, int index)
        {
            if (!d.IsDBNull(index))
            {
                return d.GetString(index);
            }
            else
            {
                return string.Empty;
            }
        }

        private static int GetNullableInt(SqlDataReader d, int index)
        {
            if (!d.IsDBNull(index))
            {
                return d.GetInt32(index);
            }
            else
            {
                return 0;
            }
        }


        private static XmlDocument GetApiXml(string apiUrl)
        {
            var wr = WebRequest.Create(apiUrl);
            var response = wr.GetResponse();


            using (var sr = new StreamReader(response.GetResponseStream()))
            {

                var xmldoc = new XmlDocument();
                xmldoc.Load(sr);
                //string x = sr.ReadToEnd();
                return xmldoc;

            }

        }


    }
}
