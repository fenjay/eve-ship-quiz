﻿using System;
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
        private const string CHARSHEETURL = "https://api.eveonline.com/char/CharacterSheet.xml.aspx";

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

        public static bool SaveCharacter(EveCharacterDTO theCharacter)
        {
            var oldCharacter = GetCharacterFromLocal(theCharacter.id);
            UpsertCorpChar(theCharacter,oldCharacter, false);
            return true;
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

        public static EveCorpDTO GetCorpInfo(int corpId)
        {
            var sql = "select CorpID, CorpName, CorpLogoUrl, AllianceID, AllianceName, AllianceLogoUrl from CS_FJL_UserCorpInfo where CorpID = " + corpId.ToString();

            var sqlConn = new SqlConnection(eve_api.GetConnectionString());
            var sqlCmd = new SqlCommand();
            var corpRetrieved = new EveCorpDTO();
            corpRetrieved.CorpID = corpId;

            try
            {
                sqlConn.Open();
                sqlCmd.CommandText = sql;
                sqlCmd.Connection = sqlConn;
                var corpReader = sqlCmd.ExecuteReader();
                if (corpReader.Read())
                {
                    corpRetrieved.CorpName = corpReader.GetString(1);
                    corpRetrieved.CorpLogoUrl = GetNullableString(corpReader,2);
                    corpRetrieved.AllianceID = GetNullableInt(corpReader, 3);
                    corpRetrieved.AllianceName = GetNullableString(corpReader, 4);
                    corpRetrieved.AllianceLogoUrl = GetNullableString(corpReader, 5);

                }
            }
            finally
            {
                sqlConn.Close();
            }

            return corpRetrieved;
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

            var updateSet = GetCorpCharsFromCorpXml(corpxml, corpId);
            
            //foreach (var corpEntry in updateSet)
            //{
            //    System.Diagnostics.Debug.Print(corpEntry.characterName + "|" + corpEntry.characterEveID);
            //}
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
            var prams = new List<SqlParameter>();

            //TODO: update this to use SQLParameters. also doesn't update empty strings well

            if (theOldCharacter != null) //update
            {
                var updateAvailable = false;
                sql.Append("update CS_FJL_CharacterInfo set ");
                
                //if updating from API, do not set the "extra" info such as comments
                //if not (i.e. updating from combined) then update all

                if (theNewCharacter.corpID != theOldCharacter.corpID) {
                    var tp = new SqlParameter("@corpID", System.Data.SqlDbType.Int);
                    tp.Value = theNewCharacter.corpID;
                    prams.Add(tp);
                    sql.Append("CorpId=@corpID");
                    //sql.Append(theNewCharacter.corpID.ToString());

                    sql.Append(",");
                    updateAvailable = true;
                }

                //characterId will presumably never update

                //null is equivalent to "" for our purposes. therefore theNewCharacter.AnyVarcharValue != null ? theNewCharacter.value : string.Empty;

             if (theNewCharacter.rankTitles != theOldCharacter.rankTitles)
                {
                    var tp = new SqlParameter("@titlesRanks", System.Data.SqlDbType.VarChar);
                    tp.Value = theNewCharacter.rankTitles != null ? theNewCharacter.rankTitles : string.Empty;
                    tp.Direction = System.Data.ParameterDirection.Input;
                    prams.Add(tp);

                    sql.Append("TitlesRanks=@titlesRanks");
                    //sql.Append(theNewCharacter.rankTitles);
                    sql.Append(",");
                    updateAvailable = true;
                }

             if (!fromApiOnly)
             {

                 if (theNewCharacter.characterApiID != theOldCharacter.characterApiID)
                 {
                     var tp = new SqlParameter("@characterapi", System.Data.SqlDbType.VarChar);
                     tp.Value = theNewCharacter.characterApiID != null ? theNewCharacter.characterApiID : string.Empty;
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("CharacterApiId=@characterapi");
                     
                     //sql.Append(theNewCharacter.characterApiID);
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.characterApiVcode != theOldCharacter.characterApiVcode)
                 {
                     var tp = new SqlParameter("@charactervcode", System.Data.SqlDbType.VarChar);
                     tp.Value = theNewCharacter.characterApiVcode != null ? theNewCharacter.characterApiVcode : string.Empty;
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("CharacterApiVcode=@charactervcode");
                     //sql.Append(theNewCharacter.characterApiVcode);
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.prospect != theOldCharacter.prospect)
                 {
                     var tp = new SqlParameter("@prospect", System.Data.SqlDbType.Bit);
                     tp.Value = theNewCharacter.prospect ? 1 : 0;
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("IsProspect=@prospect");
                     //sql.Append(theNewCharacter.prospect ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.formerMember != theOldCharacter.formerMember)
                 {
                     var tp = new SqlParameter("@former", System.Data.SqlDbType.Bit);
                     tp.Value = theNewCharacter.formerMember ? 1 : 0;
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("IsFormerMember=@former");
                     //sql.Append(theNewCharacter.formerMember ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.currentMember != theOldCharacter.currentMember)
                 {
                     var tp = new SqlParameter("@current", System.Data.SqlDbType.Bit);
                     tp.Value = theNewCharacter.currentMember ? 1 : 0;
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("IsCurrentMember=@current");
                     //sql.Append(theNewCharacter.currentMember ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.blacklist != theOldCharacter.blacklist)
                 {
                     var tp = new SqlParameter("@blacklist", System.Data.SqlDbType.Bit);
                     tp.Value = theNewCharacter.blacklist ? 1 : 0;
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("IsBlacklisted=@blacklist");
                     //sql.Append(theNewCharacter.blacklist ? "1" : "0");
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.altMainCharacterId != theOldCharacter.altMainCharacterId)
                 {
                     var tp = new SqlParameter("@altmainid", System.Data.SqlDbType.Int);
                     tp.Value = theNewCharacter.altMainCharacterId;
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("AltMainCharacterId=@altmainid");
                     //sql.Append(theNewCharacter.altMainCharacterId.ToString());
                     sql.Append(",");
                     updateAvailable = true;
                 }

                 if (theNewCharacter.comments != theOldCharacter.comments)
                 {
                     var tp = new SqlParameter("@comments", System.Data.SqlDbType.VarChar);
                     tp.Value = theNewCharacter.comments != null ? theNewCharacter.comments : string.Empty; 
                     tp.Direction = System.Data.ParameterDirection.Input;
                     prams.Add(tp);

                     sql.Append("Comments=@comments");
                     //sql.Append(theNewCharacter.comments);
                     sql.Append(",");
                     updateAvailable = true;
                 }
             }
                
                sql.Remove(sql.Length - 1, 1); //remove comma

                var charIdParam = new SqlParameter("@characterid",System.Data.SqlDbType.Int);
                charIdParam.Value = theNewCharacter.id;
                charIdParam.Direction = System.Data.ParameterDirection.Input;
                prams.Add(charIdParam);

                sql.Append(" where CharacterInfoId = @characterid");
                
                if (!updateAvailable) { return; } //bail out, no update to this one
            }
            else //insert
            {
                //TODO: update to use sql parameters. test using blacklist mx page
                sql.Append("insert into CS_FJL_CharacterInfo "); 
                sql.Append("(CorpID,CharacterName,CharacterEveId,CharacterApiId,CharacterApiVcode,TitlesRanks,IsProspect,IsFormerMember,IsCurrentMember,IsBlacklisted,AltMainCharacterId,Comments)");
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
            sqlCmd.CommandText = sql.ToString();
            foreach (var p in prams)
            {
                sqlCmd.Parameters.Add(p);
            }

            System.Diagnostics.Debug.Write(sql.ToString());
            try
            {
                sqlConn.Open();
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

            return TagApiVcode(masterApi, masterVcode);
            //return "?keyID=" + masterApi + "&vcode=" + masterVcode;

        }

        /// <summary>
        /// Refresh the character from the Eve API. Requires existing character.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public static bool RefreshCharacterFromAPI(int characterId)
        {
            var existingCharData = new EveCharacterDTO();
            existingCharData = GetCharacterFromLocal(characterId);

            var dataFromApi = new EveCharacterDTO();

            //TODO: get xml data, update
            //possibly write a generic xml parser returning name/value pairs and pluck what we want from the object thus created
            //also consolidate api requests to easily swap to CREST, add user-agent, etc.

            return true;
        }


        public static bool IsCharacterAPIExpired(string characterId, string apiId, string vCode)
        {
            var outXml = new XmlDocument();
            //https://api.eveonline.com/char/CharacterSheet.xml.aspx?keyID=3738439&vcode=zxapPGZOy5CLDv5g8WaSB6qIF1ap6v3nOMo0n6g1lVB8QzyhietdD2jVMp0VWsQS&characterID=216517242

            var charSheetURL = CHARSHEETURL + TagApiVcode(apiId, vCode) + "&characterID=" + characterId;
            var charSheetDoc = GetApiXml(charSheetURL);

            if (IsApiExpired(charSheetDoc))
            {
                return true;
            }
            else
            {
                return false;
            }
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


             private static string TagApiVcode(string apiId, string vCode)
        {
            return "?keyID=" + apiId + "&vcode=" + vCode;
        }

 

        //------------------------- Get DTOs from API XML --------------------------------------------------

             private static XmlDocument GetApiXml(string apiUrl)
             {
                 try
                 {
                     var wr = WebRequest.Create(apiUrl);
                     //wr.Headers.Add(HttpRequestHeader.UserAgent, "fenjaylabs.com/CorpSecurity/0.1");

                     var response = wr.GetResponse();

                     using (var sr = new StreamReader(response.GetResponseStream()))
                     {

                         var xmldoc = new XmlDocument();
                         xmldoc.Load(sr);
                         //string x = sr.ReadToEnd();
                         return xmldoc;

                     }
                 }
                 catch (WebException ex)
                 {
                     System.Diagnostics.Debug.Print("Web exception " + ((HttpWebResponse)ex.Response).StatusCode.ToString());
                     return new XmlDocument();
                 }

             }


        private static EveCharacterDTO GetCharDtoFromXml(XmlDocument CharApiXml)
        {
            var theDTO = new EveCharacterDTO();



            return theDTO;
        }

        private static EveCorpDTO GetCorpDtoFromXml(XmlDocument CorpApiXml)
        {
            var theDTO = new EveCorpDTO();



            return theDTO;
        }

        private static List<EveCharacterDTO> GetCorpCharsFromCorpXml(XmlDocument CorpCharsXml, int corpId)
        {
            //copy each record into a DTO
            //foreach dto: if existing, update.
            //             if doesn't exist, create
            //             if in table but not DTO list, mark as former member and clear current member. otherwise update
            var nav = CorpCharsXml.CreateNavigator();
            var updateTime = DateTime.Now;
            var updateSet = new List<EveCharacterDTO>();

            //TODO: get all the data and ignore extra

            nav.MoveToRoot();
            nav.MoveToFirstChild(); //eveapi
            nav.MoveToFirstChild(); //currenttime
            if (nav.Name == "currentTime")
            {
                updateTime = DateTime.Parse(nav.Value);
                nav.MoveToNext(); //result
            }
            if (nav.Name == "result")
            {
                nav.MoveToFirstChild(); //rowset
                if (nav.Name == "rowset")
                {
                    nav.MoveToFirstChild();  //row(s)
                    if (nav.Name == "row")
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


            return updateSet;

        }

        private static bool IsApiExpired(XmlDocument apiResult)
        {
            //TODO: step 1:  check for cache expiry
            //TODO: use https://api.eveonline.com/eve/ErrorList.xml.aspx to get errors and return something customized


            if (apiResult.ChildNodes.Count == 0) //api throws a Forbidden error, which results in empty xml
            {
                return true;
            }

            var nav = apiResult.CreateNavigator();
            nav.MoveToRoot();
            nav.MoveToFirstChild(); //eveapi
            nav.MoveToFirstChild(); //currenttime
            if (nav.Name == "currentTime")
            {
                nav.MoveToNext(); //result or error
            }
            if (nav.Name == "error")
            {
                var errorCode = nav.GetAttribute("code", string.Empty);
                if (errorCode == "222")
                {
                    return true;
                }
            }
            return false;  //might be some other error. check above api for codes
        }


    }
}
