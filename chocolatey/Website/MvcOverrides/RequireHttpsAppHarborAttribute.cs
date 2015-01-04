using System;
using System.Web.Mvc;
using System.Linq;

namespace NuGetGallery.MvcOverrides
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true,
        AllowMultiple = false)]
    public class RequireHttpsAppHarborAttribute : System.Web.Mvc.RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext.Request.IsSecureConnection)
            {
                return;
            }

            var protoHeaders = filterContext.HttpContext.Request.Headers.GetValues("X-Forwarded-Proto");
            if (protoHeaders != null)
            {
                if (string.Equals(
                    protoHeaders.FirstOrDefault(),
                    "https",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            if (filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }

            HandleNonHttpsRequest(filterContext);
        }
    }
}
