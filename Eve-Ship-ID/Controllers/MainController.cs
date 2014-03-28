using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eve_Ship_ID.Models;
using eve_api;

namespace Eve_Ship_ID.Controllers
{
    public class MainController : Controller
    {
        //
        // GET: /Main/

        public ViewResult Index(string name)
        {
            //return "Hello World" + name;
            return View();

        }

        //parameter must be called "id"
        public string NameMe(string id)
        {
            string message = "Name Me: " + id;
            return message;
        }

        //public ActionResult ShipInfo(string id)
        //{
        //    var shipType = eve_api.eve_api.GetShipType(id);
        //    var ship = new Ship { Name = id, Type = shipType };
        //    return View(ship);
        //}

        //[HttpPost]
        //public ActionResult ShipInfo(Ship s)
        //{
        //    return ShipInfo(s.Name);
        //}

        //moved to its own controller
        //public ActionResult GetQuizPage()
        //{
        //    var shipName = eve_api.eve_api.GetRandomShip(1)[0];
        //    var correctType = eve_api.eve_api.GetShipType(shipName);
        //    var shipTypes = eve_api.eve_api.GetRandomShipType(3,correctType);
        //    shipTypes.Add(correctType);


        //    var popData = new ShipQuiz { ShipName = shipName, ShipTypeOptions = shipTypes };
        //    return View(popData);
        //}
     
    }
}
