using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eve_api;
using Newtonsoft.Json.Linq;

namespace Eve_Ship_ID.Models
{
    public class VersusModel
    {

        public class ResultsDTO
        {
            /*
            # total kills in the timeframe (not losses or we have to go get total losses)
        *  # times fought, 
           # wins / losses against each other
           1v1s, 
           1v many’s, (break down by 1v2, 1v5+ , 1v10+)
           isk value
           average # of pilots on kill (each side)
           # times WCS fit
           most popular systems
           links to eve-who
        * */
            public string playerName { get; set; }
            //public string playerTwo { get; set; }
            public int nbrTotalKills { get; set; }
            //public int nbrKillsPlayerTwo { get; set; }
            public int nbr1v1s { get; set; }
            public int nbr1v2s { get; set; }
            public int nbr1vManys { get; set; }
            public int nbrTimesWon { get; set; }
            //public int nbrTimesPlayerTwoWon { get; set; }
            public int averagePilotsOnKills { get; set; }
            //public int averagePilotsOnKillPlayerTwo { get; set; }
            public string PopularSystem { get; set; }

        }


        public string CharacterOneName { get; set; }
        public int CharacterOneId { get; set; }
        
        //todo: set "verified" from client side when id/name is verified
        public bool CharacterOneVerified { get; set; }


        public string CharacterTwoName { get; set; }
        public int CharacterTwoId { get; set; }
        public bool CharacterTwoVerified { get; set; }

       // public string KillboardResults { get; set; }
        public ResultsDTO resultsOne { get; private set; }
        public ResultsDTO resultsTwo { get; private set; }

     
        public static int CharIdForName(string characterName)
        {
            return eve_api.eve_api.CharacterIdForName(characterName);
        }

        public void PopulateResultsForCharacterId(int characterIdOne, int characterIdTwo)
        {
            resultsOne = new ResultsDTO();
            resultsTwo = new ResultsDTO();
            resultsOne.playerName = CharacterOneName; 
            resultsTwo.playerName = CharacterTwoName;
            
            //it'd be cool to correct the capitalization on the name somehow...

            var KillboardResultsOne = eve_api.eve_api.GetKillboardResults(characterIdOne);
            var KillboardResultsTwo = eve_api.eve_api.GetKillboardResults(characterIdTwo);
            CalculateResults(KillboardResultsOne, resultsOne, resultsTwo.playerName);
            CalculateResults(KillboardResultsTwo, resultsTwo, resultsOne.playerName);
            
        }

        private void CalculateResults(string kbJsonResultsOne, ResultsDTO results, string opponentName)
        {
            var avgPilots = new List<int>();
            
            //----player one ---//
            var jsonObject = JArray.Parse(kbJsonResultsOne);
            foreach (var j in jsonObject)
            {
                //when stringcomparing, let's ignore case because people might type it in wrong, and the api ignores case
                if (opponentName.ToLower() == ((string)j["victim"]["characterName"]).ToLower())
                {
                    results.nbrTimesWon++;
                    if (j["attackers"].Count() == 1) { results.nbr1v1s++; }
                    if (j["attackers"].Count() == 2) { results.nbr1v2s++; }
                    if (j["attackers"].Count() > 2) { results.nbr1vManys++; }
                }
                avgPilots.Add(j["attackers"].Count());

            }

            var total = 0;
            foreach(var nbrAttackers in avgPilots)
            {
                total += nbrAttackers;
            }
            results.averagePilotsOnKills = total / avgPilots.Count;
            results.nbrTotalKills = jsonObject.Count;
        }

       
    }
}