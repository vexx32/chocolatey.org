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

using System.Net.Mime;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;

namespace NuGetGallery
{
    public partial class PagesController : Controller
    {
        private readonly IAggregateStatsService statsSvc;

        public PagesController(IAggregateStatsService statsSvc)
        {
            this.statsSvc = statsSvc;
        }

        public virtual ActionResult Home()
        {
            return View("~/Views/Pages/Home.cshtml");
        }

        public virtual ActionResult About()
        {
            return View("~/Views/Pages/About.cshtml");
        }

        public virtual ActionResult Notice()
        {
            return View("~/Views/Pages/Notice.cshtml");
        }

        public virtual ActionResult Terms()
        {
            return View("~/Views/Pages/Terms.cshtml");
        }

        public virtual ActionResult Privacy()
        {
            return View("~/Views/Pages/Privacy.cshtml");
        }

        //public ActionResult Install()
        //{
        //    return File(Url.Content("~/installChocolatey.ps1"), "text/plain");
        //}

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
        [OutputCache(VaryByParam = "None", Duration = 120, Location = OutputCacheLocation.Server)]
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
                    GoodPackages = stats.GoodPackages.ToString("n0"),
                    UpToDatePackages = stats.UpToDatePackages.ToString("n0"),
                    OlderThanOneYearPackages = stats.OlderThanOneYearPackages.ToString("n0")
                }, JsonRequestBehavior.AllowGet);
        }
    }
}
