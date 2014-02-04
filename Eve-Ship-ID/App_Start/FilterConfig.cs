using System.Web;
using System.Web.Mvc;

namespace Eve_Ship_ID
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}