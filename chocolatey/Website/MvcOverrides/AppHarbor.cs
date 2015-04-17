namespace NuGetGallery.MvcOverrides
{
    using System;
    using System.Linq;
    using System.Web;

    public static class AppHarbor
    {
        public static bool IsSecureConnection(HttpContextBase context)
        {
            //var context = HttpContext.Current;
            if (context == null) return false;

            if (context.Request.IsSecureConnection)
            {
                return true;
            }

            var protoHeaders = context.Request.Headers.GetValues("X-Forwarded-Proto");
            if (protoHeaders != null)
            {
                if (string.Equals(
                    protoHeaders.FirstOrDefault(),
                    "https",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}