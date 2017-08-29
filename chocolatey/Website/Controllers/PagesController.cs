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
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI;
using PoliteCaptcha;

namespace NuGetGallery
{
    public partial class PagesController : Controller
    {
        private readonly IAggregateStatsService statsSvc;
        private readonly IMessageService messageService;

        public PagesController(IAggregateStatsService statsSvc, IMessageService messageService)
        {
            this.statsSvc = statsSvc;
            this.messageService = messageService;
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public virtual ActionResult Home()
        {
            return View("~/Views/Pages/Home.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public virtual ActionResult Features()
        {
            return View("~/Views/Pages/Features.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public virtual ActionResult About()
        {
            return View("~/Views/Pages/About.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public virtual ActionResult Notice()
        {
            return View("~/Views/Pages/Notice.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public virtual ActionResult Terms()
        {
            return View("~/Views/Pages/Terms.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public virtual ActionResult Privacy()
        {
            return View("~/Views/Pages/Privacy.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public virtual ActionResult Pricing()
        {
            return View("~/Views/Pages/Pricing.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Business()
        {
            return View("~/Views/Pages/Business.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Kickstarter()
        {
            return View("~/Views/Pages/Kickstarter.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult MediaKit()
        {
            return View("~/Views/Pages/Media.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Company()
        {
            return View("~/Views/Pages/Company.cshtml");
        }

        [HttpGet]
        public ActionResult ContactUs()
        {
            return View("~/Views/Pages/ContactUs.cshtml", new ContactUsViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public virtual ActionResult ContactUs(ContactUsViewModel contactForm)
        {
            if (!ModelState.IsValid) return View("~/Views/Pages/ContactUs.cshtml", contactForm);
            
            var from = new MailAddress(contactForm.Email);

            var message = @"
### Contact
Name: {0} {1}
Email: {2}
Phone: {3}
Company: {4}

### Message
{5}
".format_with(contactForm.FirstName,
              contactForm.LastName,
              contactForm.Email, 
              contactForm.PhoneNumber, 
              contactForm.CompanyName, 
              contactForm.Message);

            var additionalSubject = contactForm.CompanyName;
            if (string.IsNullOrWhiteSpace(additionalSubject))
            {
                additionalSubject = "{0} {1}".format_with(contactForm.FirstName, contactForm.LastName);
            }

            messageService.ContactUs(from, contactForm.MessageTo, message, additionalSubject);

            TempData["Message"] = "Your message has been sent. You may receive follow up emails from '{0}', so make any necessary adjustments to spam filters.".format_with(Configuration.ReadAppSettings("ContactUsEmail"));

            return View("~/Views/Pages/Thanks.cshtml");
        }    
        
        [HttpGet]
        public ActionResult Discount()
        {
            return View("~/Views/Pages/Discount.cshtml", new DiscountViewModel());
        }

        readonly Regex _studentEmailAddressRegex = new Regex(@".*\.edu(\.\w{2})?$|.*\.ac.uk$|.*k12\.\w{2}\.us$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public virtual ActionResult Discount(DiscountViewModel discountForm)
        {
            if (!ModelState.IsValid) return View("~/Views/Pages/Discount.cshtml", discountForm);

            if (discountForm.DiscountType == "StudentDiscount" && !_studentEmailAddressRegex.IsMatch(discountForm.Email))
            {
                ModelState.AddModelError(string.Empty, "You must use an email ending in '.edu' or 'ac.uk' for student discount \"self-service\". If your educational institution email address doesn't end in one of these, please reach out through the Countact Us (link in bottom navigation) and choose 'Student Discount'.");
                return View("~/Views/Pages/Discount.cshtml", discountForm);
            }

            var discountLink = string.Empty;
            try
            {
               discountLink = Configuration.ReadAppSettings(discountForm.DiscountType);
            }
            catch (Exception)
            {
                // hackers
            }

            var message = @"
Hello {0},

Thanks for requesting a discount. Please see the link below:

[Discount Link]({1})".format_with(discountForm.FirstName, discountLink);

            messageService.Discount(message, discountForm.Email, "{0} {1}".format_with(discountForm.FirstName, discountForm.LastName), discountForm.DiscountType);

            TempData["Message"] = "Check your inbox! You should receive an email with more information momentarily! Email is sent from '{0}', so make any necessary adjustments to spam filters.".format_with(Configuration.ReadAppSettings("ContactUsEmail"));

            return View("~/Views/Pages/Thanks.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Support()
        {
            return View("~/Views/Pages/Support.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult ReportIssue()
        {
            return View("~/Views/Pages/ReportIssue.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Press()
        {
            return View("~/Views/Pages/Press.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Partner()
        {
            return View("~/Views/Pages/Partner.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Install()
        {
            return View("~/Views/Documentation/Installation.cshtml","~/Views/Shared/CodeLayout.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult FAQ()
        {
            return View("~/Views/Documentation/ChocolateyFAQs.cshtml", "~/Views/Shared/CodeLayout.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public ActionResult Security()
        {
            return View("~/Views/Documentation/Security.cshtml", "~/Views/Shared/CodeLayout.cshtml");
        }

        [HttpGet, OutputCache(CacheProfile = "Cache_2Hours")]
        public FileResult InstallerBatchFile()
        {
            const string batchFile = @"@echo off
SET DIR=%~dp0%

%systemroot%\System32\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ""((new-object net.webclient).DownloadFile('https://chocolatey.org/install.ps1','install.ps1'))""
%systemroot%\System32\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ""& '%DIR%install.ps1' %*""
SET PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin";

            var contentDisposition = new ContentDisposition
            {
                FileName = "installChocolatey.cmd",
                Inline = true,
            };
            Response.AppendHeader("Content-Disposition", contentDisposition.ToString());
            return File(Encoding.ASCII.GetBytes(batchFile), "text/plain");
        }

        [HttpGet]
        [OutputCache(VaryByParam = "none", Duration = 120, Location = OutputCacheLocation.Server)]
        public virtual JsonResult Stats()
        {
            var stats = statsSvc.GetAggregateStats();
            return Json(
                new
                {
                    Downloads = stats.Downloads.ToString("n0"),
                    UniquePackages = stats.UniquePackages.ToString("n0"),
                    TotalPackages = stats.TotalPackages.ToString("n0"),
                    PackagesReadyForReviewModeration = stats.PackagesReadyForReviewModeration.ToString("n0"),
                    TotalPackagesInModeration = stats.TotalPackagesInModeration.ToString("n0"),
                    AverageModerationWaitTimeHours = stats.AverageModerationWaitTimeHours.ToString("n0"),
                    UpToDatePackages = stats.UpToDatePackages.ToString("n0"),
                    OlderThanOneYearPackages = stats.OlderThanOneYearPackages.ToString("n0"),
                    ApprovedPackages = stats.ApprovedPackages.ToString("n0"), 
                    TotalApprovedPackages = stats.TotalApprovedPackages.ToString("n0"), 
                    ManuallyApprovedPackages = stats.ManuallyApprovedPackages.ToString("n0"), 
                    TotalManuallyApprovedPackages = stats.TotalManuallyApprovedPackages.ToString("n0"), 
                    TrustedPackages = stats.TrustedPackages.ToString("n0"), 
                    TotalTrustedPackages = stats.TotalTrustedPackages.ToString("n0"), 
                    TotalRejectedPackages = stats.TotalRejectedPackages.ToString("n0"), 
                    ExemptedPackages = stats.ExemptedPackages.ToString("n0"), 
                    TotalExemptedPackages = stats.TotalExemptedPackages.ToString("n0"), 
                    UnknownPackages = stats.UnknownPackages.ToString("n0"), 
                    TotalUnknownPackages = stats.TotalUnknownPackages.ToString("n0"), 
                    LatestPackagePrerelease = stats.LatestPackagePrerelease.ToString("n0"), 
                    TotalUnlistedPackages = stats.TotalUnlistedPackages.ToString("n0"), 
                    PackagesWithPackageSource = stats.PackagesWithPackageSource.ToString("n0"), 
                    PackagesPassingVerification = stats.PackagesPassingVerification.ToString("n0"), 
                    PackagesFailingVerification = stats.PackagesFailingVerification.ToString("n0"), 
                    PackagesPassingValidation = stats.PackagesPassingValidation.ToString("n0"), 
                    PackagesFailingValidation = stats.PackagesFailingValidation.ToString("n0"), 
                    PackagesCached = stats.PackagesCached.ToString("n0"), 
                    TotalPackagesCached = stats.TotalPackagesCached.ToString("n0"), 
                    PackagesCachedAvailable = stats.PackagesCachedAvailable.ToString("n0"), 
                    TotalPackagesCachedAvailable = stats.TotalPackagesCachedAvailable.ToString("n0"), 
                    PackagesCachedInvestigate = stats.PackagesCachedInvestigate.ToString("n0"), 
                    TotalPackagesCachedInvestigate = stats.TotalPackagesCachedInvestigate.ToString("n0"), 
                    PackagesScanned = stats.PackagesScanned.ToString("n0"), 
                    TotalPackagesScanned = stats.TotalPackagesScanned.ToString("n0"), 
                    PackagesScannedNotFlagged = stats.PackagesScannedNotFlagged.ToString("n0"), 
                    TotalPackagesScannedNotFlagged = stats.TotalPackagesScannedNotFlagged.ToString("n0"), 
                    PackagesScannedFlagged = stats.PackagesScannedFlagged.ToString("n0"), 
                    TotalPackagesScannedFlagged = stats.TotalPackagesScannedFlagged.ToString("n0"), 
                    PackagesScannedExempted = stats.PackagesScannedExempted.ToString("n0"), 
                    TotalPackagesScannedExempted = stats.TotalPackagesScannedExempted.ToString("n0"), 
                    PackagesScannedInvestigate = stats.PackagesScannedInvestigate.ToString("n0"), 
                    TotalPackagesScannedInvestigate = stats.TotalPackagesScannedInvestigate.ToString("n0"), 
                    TotalFileScanOverlaps = stats.TotalFileScanOverlaps.ToString("n0"), 
                    TotalFileScans = stats.TotalFileScans.ToString("n0")
                }, JsonRequestBehavior.AllowGet);
        }
    }
}
