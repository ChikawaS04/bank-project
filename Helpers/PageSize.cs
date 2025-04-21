using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System;

namespace BankApp.Helpers
{
    public static class PageSizeHelper
    {
        public static int SetPageSize(HttpContext httpContext, int? pageSizeID, string controllerName)
        {
            int pageSize = 0;
            if (pageSizeID.HasValue)
            {
                pageSize = pageSizeID.GetValueOrDefault();
                CookieHelper.CookieSet(httpContext, controllerName + "PageSizeValue", pageSize.ToString(), 480);
                CookieHelper.CookieSet(httpContext, "DefaultPageSizeValue", pageSize.ToString(), 480);
            }
            else
            {
                int.TryParse(httpContext.Request.Cookies[controllerName + "PageSizeValue"], out pageSize);
            }

            if (pageSize == 0)
            {
                int.TryParse(httpContext.Request.Cookies["DefaultPageSizeValue"], out pageSize);
            }

            return (pageSize == 0) ? 5 : pageSize;
        }

        public static SelectList PageSizeList(int? pageSize)
        {
            return new SelectList(new[] { "3", "5", "10", "20", "30", "40", "50", "100", "500" }, pageSize?.ToString());
        }
    }
}
