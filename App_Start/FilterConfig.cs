using PaperPlanner_Application.Filters;
using System.Web;
using System.Web.Mvc;

namespace PaperPlanner_Application
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Adding the New Filter for Session Authentication
            filters.Add(new SessionAuthenticationFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
