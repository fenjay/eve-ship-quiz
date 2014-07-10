using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eve_Ship_ID.Models;
using eve_api;
using System.Text;

namespace Eve_Ship_ID.Controllers
{
    public class EveShipIDQuizController : Controller
    {
        private const int QUESTIONCOUNT = 4;
        //
        // GET: /EveShipIDQuiz/

        public ActionResult Index()
        {

            //Request.ServerVariables["HTTP_REFERER"]
            if (this.HttpContext.Request.QueryString["resetquiz"] == "1")
            {
                Request.Cookies.Remove("eveshipid.score");
            }

            //delete old cookies since score has changed?

            var cookie = Request.Cookies["eveshipid.score"];
            var score = cookie == null ? string.Empty : cookie.Value;


            var popData = new ShipQuiz();

            if (score != String.Empty)
            {
                popData = getSingleQuestion(eve_api.eve_api.QuizLevel.NORMAL,ParseOutPreviousTypeIds(score));
                popData.score = score;
            }
            else
            {
                popData = getSingleQuestion(eve_api.eve_api.QuizLevel.NORMAL);
            }

            var difficultyCookie = Request.Cookies["eveshipid.difficulty"];
            var difficulty = difficultyCookie == null ? "00" : difficultyCookie.Value;

            if (difficulty.Length == 2)
            {
                if (difficulty.Substring(0, 1) == "1")
                {
                    popData.quizDifficultyCaps = true;
                }
                if (difficulty.Substring(1, 1) == "1")
                {
                    popData.quizDifficultyPirateRare = true;
                }
            }
           
            return View(popData);
        }

        [HttpPost]
        public ActionResult Index(ShipQuiz quiz)
        {

            var endValue = quiz.endValue;
            
            var newCookie = new HttpCookie("eveshipid.score", endValue);
            var existingCookie = Request.Cookies["eveshipid.score"];

            if (existingCookie != null)
            {
                Response.SetCookie(newCookie);
            }
            else
            {
                Response.Cookies.Add(newCookie);
            }

            var difficultyString = new StringBuilder();
            difficultyString.Append(quiz.quizDifficultyCaps ? "1":"0"); 
            difficultyString.Append(quiz.quizDifficultyPirateRare ? "1":"0");

            var newDifficultCookie = new HttpCookie("eveshipid.difficulty", difficultyString.ToString());
            Response.SetCookie(newDifficultCookie);

            var previousIds = ParseOutPreviousTypeIds(endValue);

            var quizDifficulty = GetQuizLevelFromString(difficultyString.ToString());

            var popData = getSingleQuestion(quizDifficulty, previousIds);
            popData.score = endValue;
            return View(popData);
            
        }

        public string GetCorrectAnswer(string id)
        {
            //id is shipname. return value is shiptype
            try
            {
                var correctType = eve_api.eve_api.GetShipType(id);
                return correctType;
            }
            catch (Exception x)
            {
                return "Error: Ship Not Found";
            }
        }

        public string GetShipId(string id)
        {
            //id is shipname. return value is shipId
            try
            {
                var shipTypeId = eve_api.eve_api.GetShipId(id);
                return shipTypeId.ToString();
            }
            catch (Exception)
            {
                return "Error: Ship Not Found";
            }
        }

        /// <summary>
        /// Returns a list of ship names in same order as the list of ship IDs
        /// </summary>
        /// <param name="id">CSV of ship ids</param>
        /// <returns>CSV of ship names</returns>
        public string GetShipNames(string id)
        {
            try
            {
                var shipNames = eve_api.eve_api.GetShipNameCSV(id);
                return shipNames;
            }
            catch (Exception)
            {
                return "Error: ship(s) not found";
            }

        }

        public string GetShipDescription(string id)
        {
            //id is shipname. return value is shipId
            try
            {
                var shipTypeId = eve_api.eve_api.GetShipDescription(id);
                return shipTypeId;
            }
            catch (Exception)
            {
                return "Error: Ship Not Found";
            }
        }

        private ShipQuiz getSingleQuestion(eve_api.eve_api.QuizLevel quizLevel)
        {
            return getSingleQuestion(quizLevel, null);
        }

        private ShipQuiz getSingleQuestion(eve_api.eve_api.QuizLevel quizLevel, List<int> alreadyAnswered)
        {
            var rand = new Random();
            var maxIdx = QUESTIONCOUNT-1;
            var correctItem = rand.Next(0, maxIdx+1); //random.Next upper bound is exclusive
            System.Diagnostics.Debug.Print("Correct item is " + correctItem);

            var shipName = eve_api.eve_api.GetRandomShip(1,alreadyAnswered, quizLevel)[0];
            var correctType = new List<string>();
            correctType.Add(eve_api.eve_api.GetShipType(shipName)); //this will be our correct answer
            
            var rawShipTypes = eve_api.eve_api.GetRandomShipType(maxIdx, quizLevel,null,null,correctType);
            
            var shipTypes = new List<string>();
            var j = 0;
            foreach(var s in rawShipTypes)
            {
                if (j != correctItem)
                {
                    shipTypes.Add(s);
                }
                else
                {
                    shipTypes.Add(correctType[0]);
                    shipTypes.Add(s);
                }
                j++;
            }
            if (correctItem == maxIdx) //it's the last option, so was not hit in the foreach above
            {
                shipTypes.Add(correctType[0]);
            }

            return new ShipQuiz { ShipName = shipName, ShipTypeOptions = shipTypes };
        }

        private List<int> ParseOutPreviousTypeIds(string scoreStr)
        {
            var previousIds = new List<int>();


            if (scoreStr.Length > 0)
            {
                var previousIdArray = scoreStr.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < previousIdArray.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        var id = 0;
                        if (Int32.TryParse(previousIdArray[i], out id)) 
                        { 
                            previousIds.Add(id); 
                        }
                    }
                }
            }
            return previousIds;
        }

        /// <summary>
        /// Takes a 2-character string and parses to Quiz level. e.g. 00 for normal, 10 for Caps, 01 for pirate/rare, 11 for all three)
        /// </summary>
        /// <param name="quizLevel"></param>
        private eve_api.eve_api.QuizLevel GetQuizLevelFromString(string quizLevel)
        {
            var ql = eve_api.eve_api.QuizLevel.NORMAL;

            if (quizLevel.Length == 2)
            {
                var caps = (quizLevel.Substring(0, 1) == "1");
                var pirate = (quizLevel.Substring(1, 1) == "1");
                if (caps && pirate) { ql = eve_api.eve_api.QuizLevel.CAPSPIRATE; }
                if (caps && !pirate) { ql = eve_api.eve_api.QuizLevel.CAPS; }
                if (!caps && pirate) { ql = eve_api.eve_api.QuizLevel.PIRATE; }
                if (!caps && !pirate) { ql = eve_api.eve_api.QuizLevel.NORMAL; }
            }

            return ql;
        }

       
    }
}
