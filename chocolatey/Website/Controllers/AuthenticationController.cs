using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NuGetGallery.MvcOverrides;

namespace NuGetGallery
{
    public partial class AuthenticationController : Controller
    {
        private readonly IFormsAuthenticationService formsAuthSvc;
        private readonly IUserService userSvc;

        public AuthenticationController(
            IFormsAuthenticationService formsAuthSvc,
            IUserService userSvc)
        {
            this.formsAuthSvc = formsAuthSvc;
            this.userSvc = userSvc;
        }

        [RequireHttpsAppHarbor]
        public virtual ActionResult LogOn()
        {
            if (TempData.ContainsKey(Constants.ReturnUrlViewDataKey))
            {
                ViewData[Constants.ReturnUrlViewDataKey] = TempData[Constants.ReturnUrlViewDataKey];
            }
            
            if (Request.IsAuthenticated)
            {
                return SafeRedirect(ViewData[Constants.ReturnUrlViewDataKey] as string);
            }

            return View();
        }

        [HttpPost, RequireHttpsAppHarbor]
        public virtual ActionResult LogOn(SignInRequest request, string returnUrl)
        {
            // TODO: improve the styling of the validation summary
            // TODO: modify the Object.cshtml partial to make the first text box autofocus, or use additional metadata

            ViewData[Constants.ReturnUrlViewDataKey] = returnUrl;

            if (!ModelState.IsValid) return View();

            var user = userSvc.FindByUsernameOrEmailAddressAndPassword(
                request.UserNameOrEmail,
                request.Password);

            if (user == null)
            {
                ModelState.AddModelError(
                    String.Empty,
                    Strings.UserNotFound);

                return View();
            }

            if (!user.Confirmed)
            {
                ViewBag.ConfirmationRequired = true;
                return View();
            }

            IEnumerable<string> roles = null;
            if (user.Roles.AnySafe())
            {
                roles = user.Roles.Select(r => r.Name);
            }

            formsAuthSvc.SetAuthCookie(
                user.Username,
                true,
                roles);

            return SafeRedirect(returnUrl);
        }

        public virtual ActionResult LogOff(string returnUrl)
        {
            // TODO: this should really be a POST

            formsAuthSvc.SignOut();

            return SafeRedirect(returnUrl);
        }

        [NonAction]
        public virtual ActionResult SafeRedirect(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl) 
                && returnUrl.Length > 1 
                && returnUrl.StartsWith("/")
                && !returnUrl.StartsWith("//") 
                && !returnUrl.StartsWith("/\\"))
            {
                return Redirect(returnUrl);
            }
          
            return Redirect(Url.Home());
        }
    }
}