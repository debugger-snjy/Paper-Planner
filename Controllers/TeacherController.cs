using PaperPlanner_Application.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaperPlanner_Application.Controllers
{
    public class TeacherController : Controller
    {
        // Function that will return View will have all options for the Teacher Operations and that are linked to the other controller
        // So, that's why only index view is needed here !
        // GET Request : teacher
        // Filter that makes sure that the user is logged in
        [Authorize]
        // Filter that makes sure that logged in user is Teacher Only
        [RoleAuthenticationFilter("Teacher")]
        public ActionResult Index()
        {
            return View();
        }
    }
}