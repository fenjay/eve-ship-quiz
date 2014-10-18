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

        public static Dictionary<int,EveCharacterDTO> GetCorpRoster()
        {
            var corpRoster = new Dictionary<int, EveCharacterDTO>();

            var sql = "select CharacterInfoId, CorpID, CharacterId, CharacterName, CharacterApiId, CharacterApiVcode, TitlesRanks, IsProspect, IsFormerMember, IsCurrentMember, IsBlacklisted, AltMainCharacterId, Comments from CS_FJL_CharacterInfo c where c.CorpID = 98340372 and c.IsCurrentMember=1";

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

                    corpRoster.Add(corpListReader.GetInt32(0),dto);
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
            string CorpRosterURL = CORPROSTERURL + GetApiVcode(corpId);
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

        }

        private static string GetApiVcode(int corpId)
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

        private static int? GetNullableInt(SqlDataReader d, int index)
        {
            if (!d.IsDBNull(index))
            {
                return d.GetInt32(index);
            }
            else
            {
                return null;
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
