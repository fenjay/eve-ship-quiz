using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eve_Ship_ID.Models;
using eve_api;

namespace Eve_Ship_ID.Controllers
{
    public class GetQuizPageController : Controller
    {
        private const int QUESTIONCOUNT = 4;
        //
        // GET: /GetQuizPage/

        public ActionResult Index()
        {
            var popData = getSingleQuestion(1);

            //Request.ServerVariables["HTTP_REFERER"]
            if (this.HttpContext.Request.QueryString["resetquiz"] == "1")
            {
                Request.Cookies.Remove("eveshipid.score");
            }

            var cookie = Request.Cookies["eveshipid.score"];
            var score = cookie == null ? string.Empty : cookie.Value;
            if (score != String.Empty)
            {
                popData.score = score;
            }

            return View(popData);
        }

        [HttpPost]
        public ActionResult Index(string endValue)
        {
            var cookie = new HttpCookie("eveshipid.score", endValue);
            Response.Cookies.Add(cookie);

            var popData = getSingleQuestion(1);
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
            var rand = new Random();
            var maxIdx = QUESTIONCOUNT-1;
            var correctItem = rand.Next(0, maxIdx);

            var shipName = eve_api.eve_api.GetRandomShip(1, quizLevel)[0];
            var correctType = eve_api.eve_api.GetShipType(shipName);
            var rawShipTypes = eve_api.eve_api.GetRandomShipType(maxIdx, correctType,quizLevel);
            
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
                    shipTypes.Add(correctType);
                    shipTypes.Add(s);
                }
                j++;
            }


            return new ShipQuiz { ShipName = shipName, ShipTypeOptions = shipTypes };
        }

       
    }
}
