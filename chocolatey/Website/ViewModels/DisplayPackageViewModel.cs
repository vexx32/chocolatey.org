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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NuGet;

namespace NuGetGallery
{
    public class DisplayPackageViewModel : ListPackageItemViewModel
    {
        public DisplayPackageViewModel(Package package)
            : this(package, false)
        {
        }

        public DisplayPackageViewModel(Package package, bool isVersionHistory)
            : base(package)
        {
            Copyright = package.Copyright;
            if (!isVersionHistory)
            {
                Dependencies = new DependencySetsViewModel(package.Dependencies);
                PackageVersions = from p in package.PackageRegistration.Packages.ToList()
                                  orderby new SemanticVersion(p.Version) descending
                                  select new DisplayPackageViewModel(p, isVersionHistory: true);
            }

            IsTrusted = package.PackageRegistration.IsTrusted;

            Files = package.Files;
            DownloadCount = package.DownloadCount;

            PackageTestResultsStatus = package.PackageTestResultStatus;
            PackageTestResultsUrl = package.PackageTestResultUrl ?? string.Empty;
        }

        public DependencySetsViewModel Dependencies { get; set; }
        public IEnumerable<DisplayPackageViewModel> PackageVersions { get; set; }
        public string Copyright { get; set; }
        public PackageTestResultStatusType PackageTestResultsStatus { get; set; }
        public string PackageTestResultsUrl { get; set; }

        public bool IsLatestVersionAvailable
        {
            get
            {
                // A package can be identified as the latest available a few different ways
                // First, if it's marked as the latest stable version
                return LatestStableVersion
                       // Or if it's marked as the latest version (pre-release)
                       || LatestVersion
                       // Or if it's the current version and no version is marked as the latest (because they're all unlisted)
                       || (IsCurrent(this) && !PackageVersions.Any(p => p.LatestVersion));
            }
        }

        public bool IsInstallOrPortable
        {
            get
            {
                return Id.EndsWith(".install")
                       || Id.EndsWith(".portable")
                       || Id.EndsWith(".app")
                       || Id.EndsWith(".tool")
                       || Id.EndsWith(".commandline");
            }
        }

        public IEnumerable<PackageFile> Files { get; private set; }

        [Display(Name = "Add to Review Comments")]
        [StringLength(1000)]
        public string NewReviewComments { get; set; }

        [Display(Name = "Trust this package id?")]
        public bool IsTrusted { get; private set; }
    }
}
