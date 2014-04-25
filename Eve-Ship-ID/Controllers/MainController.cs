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

     
    }
}
