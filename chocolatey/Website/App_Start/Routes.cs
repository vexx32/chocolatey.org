// Copyright 2011 - Present RealDimensions Software, LLC, the original 
// authors/contributors from ChocolateyGallery
// at https://github.com/chocolatey/chocolatey.org,
// and the authors/contributors of NuGetGallery 
// at https://github.com/NuGet/NuGetGallery
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Web.Mvc;
using System.Web.Routing;
using MvcHaack.Ajax;
using RouteMagic;

namespace NuGetGallery
{
    public static class Routes
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.IgnoreRoute("{*Content}", new { imgs = @"(.*/)?Content(/.*)?" });
            routes.IgnoreRoute("{*Scripts}", new { scripts = @"(.*/)?Scripts(/.*)?" });

            routes.MapRoute(RouteName.Home, "", MVC.Pages.Home());

            routes.MapRoute(
                RouteName.About, "about", new
                {
                    controller = "Pages",
                    Action = "About"
                });

            routes.MapRoute(
                RouteName.InstallerBatchFile, "installChocolatey.cmd", new
                {
                    controller = "Pages",
                    Action = "InstallerBatchFile"
                });

            routes.MapRoute(
                RouteName.Notice, "notice", new
                {
                    controller = "Pages",
                    Action = "Notice"
                });

            routes.MapRoute(RouteName.Stats, "stats", MVC.Pages.Stats());

            routes.MapRoute(
                "rss feed", "feed.rss", new
                {
                    controller = "RSS",
                    Action = "feed.rss"
                });

            routes.Add(new JsonRoute("json/{controller}"));

            routes.MapRoute(RouteName.Policies, "policies/{action}", MVC.Pages.Terms());

            var packageListRoute = routes.MapRoute(RouteName.ListPackages, "packages", MVC.Packages.ListPackages());

            routes.MapRoute(
                RouteName.NotifyComment, "packages/{packageId}/notify-comment", new
                {
                    controller = MVC.Packages.Name,
                    action = "NotifyMaintainersOfAddedComment"
                });

            var uploadPackageRoute = routes.MapRoute(RouteName.UploadPackage, "packages/upload", MVC.Packages.UploadPackage());

            routes.MapRoute(RouteName.VerifyPackage, "packages/verify-upload", MVC.Packages.VerifyPackage());

            routes.MapRoute(RouteName.CancelUpload, "packages/cancel-upload", MVC.Packages.CancelUpload());

            routes.MapRoute(
                RouteName.PackageOwnerConfirmation, "packages/{id}/owners/{username}/confirm/{token}", new
                {
                    controller = MVC.Packages.Name,
                    action = "ConfirmOwner"
                });

            // We need the following two routes (rather than just one) due to Routing's 
            // Consecutive Optional Parameter bug. :(
            var packageDisplayRoute = routes.MapRoute(
                RouteName.DisplayPackage, "packages/{id}/{version}", MVC.Packages.DisplayPackage().AddRouteValue("version", UrlParameter.Optional), null /*defaults*/, new
                {
                    version = new VersionRouteConstraint()
                });

            var packageVersionActionRoute = routes.MapRoute(
                RouteName.PackageVersionAction, "packages/{id}/{version}/{action}", new
                {
                    controller = MVC.Packages.Name
                }, new
                {
                    version = new VersionRouteConstraint()
                });

            var packageActionRoute = routes.MapRoute(
                RouteName.PackageAction, "packages/{id}/{action}", new
                {
                    controller = MVC.Packages.Name
                });

            var resendRoute = routes.MapRoute("ResendConfirmation", "account/ResendConfirmation", MVC.Users.ResendConfirmation());

            //Redirecting v1 Confirmation Route
            routes.Redirect(r => r.MapRoute("v1Confirmation", "Users/Account/ChallengeEmail")).To(resendRoute);

            routes.MapRoute(
                RouteName.Authentication, "users/account/{action}", new
                {
                    controller = MVC.Authentication.Name
                });

            routes.MapRoute(RouteName.Profile, "profiles/{username}", MVC.Users.Profiles());

            routes.MapRoute(RouteName.PasswordReset, "account/{action}/{username}/{token}", MVC.Users.ResetPassword());

            routes.MapRoute(RouteName.Account, "account/{action}", MVC.Users.Account());

            routes.MapRoute(
                "site" + RouteName.DownloadPackage, "packages/{id}/{version}/DownloadPackage", MVC.Api.GetPackage(), defaults: new
                {
                    version = UrlParameter.Optional
                }, constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET")
                });
            
            // V1 Routes
            routes.MapRoute("v1Legacy" + RouteName.PushPackageApi, "PackageFiles/{apiKey}/nupkg", MVC.Api.CreatePackagePost());
            routes.MapRoute("v1Legacy" + RouteName.PublishPackageApi, "PublishedPackages/Publish", MVC.Api.PublishPackage());

            // V2 routes
            routes.MapRoute(
                "v2" + RouteName.VerifyPackageKey, "api/v2/verifykey/{id}/{version}", MVC.Api.VerifyPackageKey(), defaults: new
                {
                    id = UrlParameter.Optional,
                    version = UrlParameter.Optional
                });

            routes.MapRoute(
                "v2" + RouteName.DownloadPackage, "api/v2/package/{id}/{version}", MVC.Api.GetPackage(), defaults: new
                {
                    version = UrlParameter.Optional
                }, constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET")
                });

            routes.MapRoute(
                "v2" + RouteName.PushPackageApi, "api/v2/package", MVC.Api.CreatePackagePut(), defaults: null, constraints: new
                {
                    httpMethod = new HttpMethodConstraint("PUT")
                });

            routes.MapRoute(
                "v2" + RouteName.DeletePackageApi, "api/v2/package/{id}/{version}", MVC.Api.DeletePackage(), defaults: null, constraints: new
                {
                    httpMethod = new HttpMethodConstraint("DELETE")
                });

            routes.MapRoute(
                "v2" + RouteName.PublishPackageApi, "api/v2/package/{id}/{version}", MVC.Api.PublishPackage(), defaults: null, constraints: new
                {
                    httpMethod = new HttpMethodConstraint("POST")
                });

            routes.MapServiceRoute(RouteName.V2ApiSubmittedFeed, "api/v2/submitted", typeof(V2SubmittedFeed));

            //routes.MapRoute("v2" + RouteName.TestPackageApi, "api/v2/test/{id}/{version}",
            //     new {controller = "Api", action = "TestPackageApi"}
            //     );

            routes.MapRoute(
                 "v2" + RouteName.TestPackageApi, 
                 "api/v2/test/{id}/{version}", 
                 MVC.Api.TestPackage(), 
                 defaults: null,
                 constraints: new
                 {
                    httpMethod = new HttpMethodConstraint("POST")
                 }
                 );

            routes.MapRoute("v2PackageIds", "api/v2/package-ids", MVC.Api.GetPackageIds());

            routes.MapRoute("v2PackageVersions", "api/v2/package-versions/{id}", MVC.Api.GetPackageVersions());

            routes.MapServiceRoute(RouteName.V2ApiFeed, "api/v2/", typeof(V2Feed));

            // Redirected Legacy Routes

            routes.Redirect(r => r.MapRoute("ReportAbuse", "Package/ReportAbuse/{id}/{version}", MVC.Packages.ReportAbuse()), permanent: true).To(packageVersionActionRoute);

            routes.Redirect(
                r => r.MapRoute(
                    "PackageActions", "Package/{action}/{id}", MVC.Packages.ContactOwners(), null /*defaults*/, // This next bit looks bad, but it's not. It will never change because 
                    // it's mapping the legacy routes to the new better routes.
                    new
                    {
                        action = "ContactOwners|ManagePackageOwners"
                    }), permanent: true).To(packageActionRoute);

            // TODO: this route looks broken as there is no EditPackage action
            //routes.Redirect(
            //    r => r.MapRoute(
            //        "EditPackage",
            //        "Package/Edit/{id}/{version}",
            //        new { controller = PackagesController.ControllerName, action = "EditPackage" }),
            //    permanent: true).To(packageVersionActionRoute);

            routes.Redirect(r => r.MapRoute(RouteName.ListPackages, "List/Packages", MVC.Packages.ListPackages()), permanent: true).To(packageListRoute);

            routes.Redirect(r => r.MapRoute(RouteName.DisplayPackage, "List/Packages/{id}/{version}", MVC.Packages.DisplayPackage().AddRouteValue("version", UrlParameter.Optional)), permanent: true)
                  .To(packageDisplayRoute);

            routes.Redirect(r => r.MapRoute(RouteName.NewSubmission, "Contribute/NewSubmission", MVC.Packages.UploadPackage()), permanent: true).To(uploadPackageRoute);
        }
    }
}
