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
        private const int QUIZCOUNT = 4;
        //
        // GET: /GetQuizPage/

        public ActionResult Index()
        {
            var popData = getSingleQuiz(1);
            return View(popData);
        }

        [HttpPost]
        public ActionResult Index(string endValue)
        {
            var popData = getSingleQuiz(1);
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



        private ShipQuiz getSingleQuiz(int quizLevel)
        {
            var rand = new Random();
            var maxIdx = QUIZCOUNT-1;
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
