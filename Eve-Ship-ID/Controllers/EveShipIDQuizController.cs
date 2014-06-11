using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eve_Ship_ID.Models;
using eve_api;

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
                popData = getSingleQuestion(1,ParseOutPreviousTypeIds(score));
                popData.score = score;
            }
            else
            {
                popData = getSingleQuestion(1);
            }
            
            return View(popData);
        }

        [HttpPost]
        //public ActionResult Index(string endValue)
        public ActionResult Index(ShipQuiz quiz)
        {

            var endValue = quiz.endValue;
            
            var cookie = new HttpCookie("eveshipid.score", endValue);
            if (Request.Cookies.Count > 0)
            {
                Response.SetCookie(cookie);
            }
            else
            {
                Response.Cookies.Add(cookie);
            }

            var previousIds = ParseOutPreviousTypeIds(endValue);

            var popData = getSingleQuestion(1,previousIds);
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

        private ShipQuiz getSingleQuestion(int quizLevel)
        {
            return getSingleQuestion(quizLevel, null);
        }

        private ShipQuiz getSingleQuestion(int quizLevel, List<int> alreadyAnswered)
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

       
    }
}
