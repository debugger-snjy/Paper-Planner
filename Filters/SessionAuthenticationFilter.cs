using PaperPlanner_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaperPlanner_Application.Filters
{
    // Implemeted the Filter using the IAuthorizationFilter
    public class SessionAuthenticationFilter : IAuthorizationFilter
    {
        // Implemented Method
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!IsAnonymousUser(filterContext.ActionDescriptor))
            {
                // Accessing the Session
                if (HttpContext.Current.Session["tempData"] == null)
                {
                    // Store the Data or Messages in TempData
                    filterContext.Controller.TempData["AlertMsg"] = "Unauthorized access";

                    // Redirect to login page
                    filterContext.Result = new RedirectResult("/Home");
                }
            }
        }

        private bool IsAnonymousUser(ActionDescriptor actionDescriptor)
        {
            var controllerName = actionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = actionDescriptor.ActionName;

            // Check if the action is in the Home controller and is allowed to be accessed anonymously
            if (controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase))
            {
                var anonymousActions = new[] { "Index", "Signup", "About","Contact" }; // Add actions in the Home controller that don't require authentication
                return Array.Exists(anonymousActions, a => a.Equals(actionName, StringComparison.OrdinalIgnoreCase));
            }

            // For actions in other controllers, return false to require authentication
            return false;
        }
    }

}

/*
 * Steps to Add a Custom Filter in our Project : 
 * 
 * 1. Create a Custom Authentication Filter:
 *      Create a custom authentication filter by implementing the IAuthorizationFilter interface. In the 
 *      OnAuthorization method, check if the Session["tempData"] is set. If it's not set, you can redirect the user 
 *      to a login page or display an unauthorized access message.
 * 
 *      // Implemeted the Filter using the IAuthorizationFilter
 *      public class SessionAuthenticationFilter : IAuthorizationFilter
 *      {
 *          // Implemented Method
 *          public void OnAuthorization(AuthorizationContext filterContext)
 *          {
 *              // Accessing the Session
 *              if (HttpContext.Current.Session["tempData"] == null)
 *              {
 *                  // Store the Data or Messages in TempData
 *                  filterContext.Controller.TempData["ErrorMessage"] = "Unauthorized access";
 *       
 *                  // Redirect to login page
 *                  filterContext.Result = new RedirectResult("~/Home");
 *              }
 *          }
 *      }
 * 
 * 
 * 
 * 2. Register the Authentication Filter:
 *      Register the custom authentication filter globally in the FilterConfig class or apply it selectively to 
 *      specific controllers or actions where authentication is required.
 *      
 *      using System.Web.Mvc;
 *      public class FilterConfig
 *      {
 *          public static void RegisterGlobalFilters(GlobalFilterCollection filters)
 *          {
 *              filters.Add(new SessionAuthenticationFilter());
 *              // Other global filters
 *          }
 *      }
 *      
 * 3. Register the global filters in the Application_Start method of Global.asax.cs
 * 
 *      protected void Application_Start()
 *      {
 *          AreaRegistration.RegisterAllAreas();
 *          FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
 *          // Other application startup tasks
 *      }
 * 
 * 4. Apply the Filter:
 *      Apply the filter to the controllers or actions where you want to enforce authentication. You can do this by 
 *      decorating the controller or action with the [Authorize]  // Filter that makes sure that the user is logged in attribute.
 * 
 *      [Authorize]  // Filter that makes sure that the user is logged in
 *      public class HomeController : Controller
 *      {
 *          // Controller actions
 *      }
 */
