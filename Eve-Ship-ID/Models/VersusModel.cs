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
            public string playerOne { get; set; }
            //public string playerTwo { get; set; }
            public int nbrKillsPlayerOne { get; set; }
            //public int nbrKillsPlayerTwo { get; set; }
            public int nbr1v1s { get; set; }
            public int nbr1v2s { get; set; }
            public int nbr1vManys { get; set; }
            public int nberTimesPlayerOneWon { get; set; }
            //public int nbrTimesPlayerTwoWon { get; set; }
            public int averagePilotsOnKillPlayerOne { get; set; }
            //public int averagePilotsOnKillPlayerTwo { get; set; }
            public string PopularSystem1 { get; set; }

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
            resultsOne.playerOne = CharacterOneName; //maybe not needed. I already have this name accessible to view
            resultsTwo.playerOne = CharacterTwoName;

            var KillboardResultsOne = eve_api.eve_api.GetKillboardResults(characterIdOne);
            var KillboardResultsTwo = eve_api.eve_api.GetKillboardResults(characterIdOne);
            CalculateResults(KillboardResultsOne, KillboardResultsTwo);
            
        }

        /// <summary>
        /// Calculates results based on JSON returned from killboard (zkb)
        /// </summary>
        /// <param name="kbJsonResults"></param>
        /// <returns>A ResultsDTO of the calculated stuff</returns>
        private void CalculateResults(string kbJsonResultsOne, string kbJsonResultsTwo)
        {
            var jsonObject = JArray.Parse(kbJsonResultsOne);
            foreach (var j in jsonObject)
            {
                //when stringcomparing, let's ignore case because people might type it in wrong, and the api ignores case
                if (CharacterTwoName.ToLower() == ((string)j["victim"]["characterName"]).ToLower())
                {
                    resultsOne.nberTimesPlayerOneWon++;
                    if (j["attackers"].Count() == 1) { resultsOne.nbr1v1s++; }
                    if (j["attackers"].Count() == 2) { resultsOne.nbr1v2s++; }
                    if (j["attackers"].Count() > 2) { resultsOne.nbr1vManys++; }
                }

                //----player one ---//
                // total kills
                resultsOne.nbrKillsPlayerOne = jsonObject.Count;

                //nbr times 

                // return resultsOne;
            }
        }

       
    }
}