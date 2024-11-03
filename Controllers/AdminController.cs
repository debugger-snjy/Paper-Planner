using PaperPlanner_Application.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaperPlanner_Application.Controllers
{
    public class AdminController : Controller
    {

        // Function that will return View will have all options for the Admin Operations and that are linked to the other controller
        // So, that's why only index view is needed here !
        // GET Request
        // Filter that makes sure that the user is logged in
        [Authorize]
        // Filter that makes sure that the user is Admin
        [RoleAuthenticationFilter( "Admin")]
        public ActionResult Index()
        {
            return View();
        }
    }
}