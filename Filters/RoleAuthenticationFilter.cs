using PaperPlanner_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaperPlanner_Application.Filters
{
    public class RoleAuthenticationFilter : FilterAttribute, IAuthorizationFilter
    {
        private readonly string[] _rolesAllowed;

        public RoleAuthenticationFilter(params string[] rolesAllowed)
        {
            _rolesAllowed = rolesAllowed;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var record = filterContext.HttpContext.Session["tempData"]; // Assuming you store the user role in session
            var userRole = (record as User).role.ToString();
            bool isAuthorized = Array.Exists(_rolesAllowed, role => role.Equals(userRole, StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                // Store the Data or Messages in TempData
                filterContext.Controller.TempData["AlertMsg"] = "Unauthorized access";
                filterContext.Controller.TempData["AlertType"] = "failed";
                filterContext.Result = new RedirectResult("/" + (record as User).role.ToString() + "/Index");
            }
        }
    }
}