using System.Web;
using System.Web.Mvc;

namespace PSAPLCDashboard.Web.Dashboard
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
