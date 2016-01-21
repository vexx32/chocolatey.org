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

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using NuGet;

namespace NuGetGallery
{
    public partial class ApiController : AppController
    {
        private readonly IPackageService packageSvc;
        private readonly IUserService userSvc;
        private readonly IPackageFileService packageFileSvc;
        private readonly INuGetExeDownloaderService nugetExeDownloaderSvc;
        private readonly GallerySetting settings;

        public ApiController(IPackageService packageSvc, IPackageFileService packageFileSvc, IUserService userSvc, INuGetExeDownloaderService nugetExeDownloaderSvc, GallerySetting settings)
        {
            this.packageSvc = packageSvc;
            this.packageFileSvc = packageFileSvc;
            this.userSvc = userSvc;
            this.nugetExeDownloaderSvc = nugetExeDownloaderSvc;
            this.settings = settings;
        }

        [ActionName("GetPackageApi"), HttpGet]
        public virtual ActionResult GetPackage(string id, string version)
        {
            // if the version is null, the user is asking for the latest version. Presumably they don't want includePrerelease release versions. 
            // The allow prerelease flag is ignored if both partialId and version are specified.
            var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease: false);

            if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));

            packageSvc.AddDownloadStatistics(package, Request.UserHostAddress, Request.UserAgent);

            if (!string.IsNullOrWhiteSpace(package.ExternalPackageUrl)) return Redirect(package.ExternalPackageUrl);
            else return packageFileSvc.CreateDownloadPackageActionResult(package);
        }

        [ActionName("GetNuGetExeApi"), HttpGet, OutputCache(VaryByParam = "none", Location = OutputCacheLocation.ServerAndClient, Duration = 600)]
        public virtual ActionResult GetNuGetExe()
        {
            return nugetExeDownloaderSvc.CreateNuGetExeDownloadActionResult();
        }

        [ActionName("VerifyPackageKeyApi"), HttpGet]
        public virtual ActionResult VerifyPackageKey(string apiKey, string id, string version)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var user = userSvc.FindByApiKey(parsedApiKey);
            if (user == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, string.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "push"));

            if (!String.IsNullOrEmpty(id))
            {
                // If the partialId is present, then verify that the user has permission to push for the specific Id \ version combination.
                var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease: true, useCache: false);
                if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));

                if (!package.IsOwner(user)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, string.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "push"));
            }

            return new EmptyResult();
        }

        [ActionName("PushPackageApi"), HttpPut]
        public virtual ActionResult CreatePackagePut(string apiKey)
        {
            return CreatePackageInternal(apiKey);
        }

        [ActionName("PushPackageApi"), HttpPost]
        public virtual ActionResult CreatePackagePost(string apiKey)
        {
            return CreatePackageInternal(apiKey);
        }

        private ActionResult CreatePackageInternal(string apiKey)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var user = userSvc.FindByApiKey(parsedApiKey);
            if (user == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "push"));

            var packageToPush = ReadPackageFromRequest();

            // Ensure that the user can push packages for this partialId.
            var packageRegistration = packageSvc.FindPackageRegistrationById(packageToPush.Id, useCache:false);
            if (packageRegistration != null)
            {
                if (!packageRegistration.IsOwner(user)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "push"));

                var existingPackage = packageRegistration.Packages.FirstOrDefault(p => p.Version.Equals(packageToPush.Version.ToString(), StringComparison.OrdinalIgnoreCase));

                if (existingPackage != null)
                {
                    switch (existingPackage.Status)
                    {
                        case PackageStatusType.Rejected:
                            return new HttpStatusCodeWithBodyResult(
                                HttpStatusCode.Conflict, string.Format("This package has been {0} and can no longer be submitted.", existingPackage.Status.GetDescriptionOrValue().ToLower()));
                        case PackageStatusType.Submitted:
                            //continue on 
                            break;
                        default:
                            return new HttpStatusCodeWithBodyResult(HttpStatusCode.Conflict, String.Format(CultureInfo.CurrentCulture, Strings.PackageExistsAndCannotBeModified, packageToPush.Id, packageToPush.Version));
                    }
                }
            }

            try
            {
                packageSvc.CreatePackage(packageToPush, user);
            } catch (Exception ex)
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.Conflict, string.Format("This package had an issue pushing: {0}", ex.Message));
            }

            return new HttpStatusCodeWithBodyResult(HttpStatusCode.Created, "Package has been pushed and will show up once moderated and approved.");
        }

        [ActionName("DeletePackageApi"), HttpDelete]
        public virtual ActionResult DeletePackage(string apiKey, string id, string version)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var user = userSvc.FindByApiKey(parsedApiKey);
            if (user == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, string.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "delete"));

            var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease: true, useCache: false);
            if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));

            if (!package.IsOwner(user)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, string.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "delete"));

            packageSvc.MarkPackageUnlisted(package);
            return new EmptyResult();
        }

        [ActionName("PublishPackageApi"), HttpPost]
        public virtual ActionResult PublishPackage(string apiKey, string id, string version)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var user = userSvc.FindByApiKey(parsedApiKey);
            if (user == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, string.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "publish"));

            var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease: true, useCache: false);
            if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));
            if (!package.IsOwner(user)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, string.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "publish"));

            packageSvc.MarkPackageListed(package);

            return new HttpStatusCodeWithBodyResult(HttpStatusCode.Accepted, "Package has been accepted and will show up once moderated and approved.");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            var exception = filterContext.Exception;
            var request = filterContext.HttpContext.Request;
            filterContext.Result = new HttpStatusCodeWithBodyResult(HttpStatusCode.InternalServerError, exception.Message, request.IsLocal ? exception.StackTrace : exception.Message);
        }

        protected internal virtual IPackage ReadPackageFromRequest()
        {
            Stream stream;
            if (Request.Files.Count > 0)
            {
                // If we're using the newer API, the package stream is sent as a file.
                stream = Request.Files[0].InputStream;
            } else stream = Request.InputStream;

            return new ZipPackage(stream);
        }

        [ActionName("PackageIDs"), HttpGet]
        public virtual ActionResult GetPackageIds(string partialId, bool? includePrerelease)
        {
            var qry = GetService<IPackageIdsQuery>();
            return new JsonNetResult(qry.Execute(partialId, includePrerelease).ToArray());
        }

        [ActionName("PackageVersions"), HttpGet]
        public virtual ActionResult GetPackageVersions(string id, bool? includePrerelease)
        {
            var qry = GetService<IPackageVersionsQuery>();
            return new JsonNetResult(qry.Execute(id, includePrerelease).ToArray());
        }

        [ActionName("TestPackageApi"), HttpPost]
        public virtual ActionResult SubmitPackageTestResults(string apiKey, string id, string version, bool success, string resultDetailsUrl)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var testReporterUser = userSvc.FindByApiKey(parsedApiKey);
            if (testReporterUser == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submittestresults"));
            // Only the package operations user can submit test results
            if (testReporterUser.Key != settings.PackageOperationsUserKey) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submittestresults"));

            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(version))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));
            }
          
            if (string.IsNullOrWhiteSpace(resultDetailsUrl))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, "Submitting test results requires 'resultDetailsUrl' and 'success'.");
            }

            try
            {
                var tempUri = new Uri(resultDetailsUrl);
            }
            catch (Exception)
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, "Submitting test results requires 'resultDetailsUrl' to be a Url.");
            }

            var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease:true, useCache:false);
            if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));
            
            packageSvc.ChangePackageTestStatus(package, success, resultDetailsUrl, testReporterUser);

            return new HttpStatusCodeWithBodyResult(HttpStatusCode.Accepted, "Package test results have been updated.");
        }
   
        [ActionName("ValidatePackageApi"), HttpPost]
        public virtual ActionResult SubmitPackageValidationResults(string apiKey, string id, string version, bool success, string validationComments)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var testReporterUser = userSvc.FindByApiKey(parsedApiKey);
            if (testReporterUser == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submitvalidationresults"));
            // Only the package operations user can submit test results
            if (testReporterUser.Key != settings.PackageOperationsUserKey) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submitvalidationresults"));

            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(version))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));
            }
          
            if (string.IsNullOrWhiteSpace(validationComments))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, "Submitting validation results requires 'validationComments' and 'success'.");
            }
            
            var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease:true, useCache:false);
            if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));

            package.PackageValidationResultDate = DateTime.UtcNow;
            package.PackageValidationResultStatus = PackageAutomatedReviewResultStatusType.Failing;

            var message = "{0} has failed automated validation.".format_with(package.PackageRegistration.Id);
            if (success)
            {
                package.PackageValidationResultStatus = PackageAutomatedReviewResultStatusType.Passing;
                message = "{0} has passed automated validation. It may have or may still fail other checks like testing (verification).".format_with(package.PackageRegistration.Id);
            }

            message += "{0}{1}".format_with(Environment.NewLine, validationComments);

            packageSvc.UpdateSubmittedStatusAfterAutomatedReviews(package);

            packageSvc.ChangePackageStatus(package, package.Status, package.ReviewComments, message, testReporterUser, testReporterUser, sendMaintainerEmail: true, submittedStatus: success ? package.SubmittedStatus : PackageSubmittedStatusType.Waiting, assignReviewer: false);

            return new HttpStatusCodeWithBodyResult(HttpStatusCode.Accepted, "Package validation results have been updated.");
        } 
        
        [ActionName("CleanupPackageApi"), HttpPost]
        public virtual ActionResult SubmitPackageCleanupResults(string apiKey, string id, string version, bool reject, string cleanupComments)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var testReporterUser = userSvc.FindByApiKey(parsedApiKey);
            if (testReporterUser == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submitcleanupresults"));
            // Only the package operations user can submit test results
            if (testReporterUser.Key != settings.PackageOperationsUserKey) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submitcleanupresults"));

            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(version))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));
            }

            if (string.IsNullOrWhiteSpace(cleanupComments))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, "Submitting cleanup results requires 'cleanupComments' and 'reject'.");
            }
            
            var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease:true, useCache:false);
            if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));

            if (!package.PackageCleanupResultDate.HasValue) package.PackageCleanupResultDate = DateTime.UtcNow;
           
            packageSvc.ChangePackageStatus(package, reject ? PackageStatusType.Rejected : package.Status, package.ReviewComments, cleanupComments, testReporterUser, testReporterUser, sendMaintainerEmail: true, submittedStatus: package.SubmittedStatus, assignReviewer: false);

            return new HttpStatusCodeWithBodyResult(HttpStatusCode.Accepted, "Package validation results have been updated.");
        }

        [ActionName("DownloadCachePackageApi"), HttpPost]
        public virtual ActionResult SubmitPackageDownloadCacheResults(string apiKey, string id, string version, bool cached, string cacheData)
        {
            Guid parsedApiKey;
            if (!Guid.TryParse(apiKey, out parsedApiKey)) return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Strings.InvalidApiKey, apiKey));

            var testReporterUser = userSvc.FindByApiKey(parsedApiKey);
            if (testReporterUser == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submitcacheresults"));
            // Only the package operations user can submit test results
            if (testReporterUser.Key != settings.PackageOperationsUserKey) return new HttpStatusCodeWithBodyResult(HttpStatusCode.Forbidden, String.Format(CultureInfo.CurrentCulture, Strings.ApiKeyNotAuthorized, "submitcacheresults"));

            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(version))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));
            }

            if (cached && string.IsNullOrWhiteSpace(cacheData))
            {
                return new HttpStatusCodeWithBodyResult(HttpStatusCode.BadRequest, "Submitting cache with 'cached'=true requires 'cacheData'.");
            }
            
            var package = packageSvc.FindPackageByIdAndVersion(id, version, allowPrerelease:true, useCache:false);
            if (package == null) return new HttpStatusCodeWithBodyResult(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Strings.PackageWithIdAndVersionNotFound, id, version));

            package.DownloadCacheDate = DateTime.UtcNow;
            package.DownloadCacheStatus = cached ? PackageDownloadCacheStatusType.Available : PackageDownloadCacheStatusType.Checked;
            if (!string.IsNullOrWhiteSpace(cacheData)) package.DownloadCache = cacheData;

            packageSvc.SaveMinorPackageChanges(package);

            return new HttpStatusCodeWithBodyResult(HttpStatusCode.Accepted, "Package validation results have been updated.");
        }

    }
}
