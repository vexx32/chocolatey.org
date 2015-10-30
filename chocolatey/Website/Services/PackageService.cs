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
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Transactions;
using Elmah;
using NuGet;
using NugetGallery;
using StackExchange.Profiling;

namespace NuGetGallery
{
    public class PackageService : IPackageService
    {
        private readonly ICryptographyService cryptoSvc;
        private readonly IEntityRepository<PackageRegistration> packageRegistrationRepo;
        private readonly IEntityRepository<Package> packageRepo;
        private readonly IEntityRepository<PackageAuthor> packageAuthorRepo;
        private readonly IEntityRepository<PackageFramework> packageFrameworksRepo;
        private readonly IEntityRepository<PackageDependency> packageDependenciesRepo;
        private readonly IEntityRepository<PackageFile> packageFilesRepo;
        private readonly IEntityRepository<PackageStatistics> packageStatsRepo;
        private readonly IPackageFileService packageFileSvc;
        private readonly IEntityRepository<PackageOwnerRequest> packageOwnerRequestRepository;
        private readonly IMessageService messageSvc;
        private readonly string submittedStatus = PackageStatusType.Submitted.GetDescriptionOrValue();

        public PackageService(
            ICryptographyService cryptoSvc,
            IEntityRepository<PackageRegistration> packageRegistrationRepo,
            IEntityRepository<Package> packageRepo,
            IEntityRepository<PackageStatistics> packageStatsRepo,
            IPackageFileService packageFileSvc,
            IEntityRepository<PackageOwnerRequest> packageOwnerRequestRepository,
            IEntityRepository<PackageAuthor> packageAuthorRepo,
            IEntityRepository<PackageFramework> packageFrameworksRepo,
            IEntityRepository<PackageDependency> packageDependenciesRepo,
            IEntityRepository<PackageFile> packageFilesRepo,
            IMessageService messageSvc)
        {
            this.cryptoSvc = cryptoSvc;
            this.packageRegistrationRepo = packageRegistrationRepo;
            this.packageRepo = packageRepo;
            this.packageStatsRepo = packageStatsRepo;
            this.packageFileSvc = packageFileSvc;
            this.packageOwnerRequestRepository = packageOwnerRequestRepository;
            this.packageAuthorRepo = packageAuthorRepo;
            this.packageFrameworksRepo = packageFrameworksRepo;
            this.packageDependenciesRepo = packageDependenciesRepo;
            this.packageFilesRepo = packageFilesRepo;
            this.messageSvc = messageSvc;
        }

        public Package CreatePackage(IPackage nugetPackage, User currentUser)
        {
            ValidateNuGetPackage(nugetPackage);

            var packageRegistration = CreateOrGetPackageRegistration(currentUser, nugetPackage);

            var package = CreatePackageFromNuGetPackage(packageRegistration, nugetPackage);
            packageRegistration.Packages.Add(package);

            using (var tx = new TransactionScope())
            {
                using (var stream = nugetPackage.GetStream())
                {
                    UpdateIsLatest(packageRegistration);
                    packageRegistrationRepo.CommitChanges();
                    packageFileSvc.SavePackageFile(package, stream);
                    tx.Complete();
                }
            }

            if (package.Status != PackageStatusType.Approved && package.Status != PackageStatusType.Exempted) NotifyForModeration(package, comments: string.Empty);

            InvalidateCache(package.PackageRegistration);
            Cache.InvalidateCacheItem(string.Format("dependentpackages-{0}", package.Key));

            return package;
        }

        public void DeletePackage(string id, string version)
        {
            var package = FindPackageByIdAndVersion(id, version, allowPrerelease:true, useCache: false);

            if (package == null) throw new EntityException(Strings.PackageWithIdAndVersionNotFound, id, version);

            using (var tx = new TransactionScope())
            {
                var packageRegistration = package.PackageRegistration;
                packageRepo.DeleteOnCommit(package);
                packageFileSvc.DeletePackageFile(id, version);
                UpdateIsLatest(packageRegistration);
                packageRepo.CommitChanges();
                if (packageRegistration.Packages.Count == 0)
                {
                    packageRegistrationRepo.DeleteOnCommit(packageRegistration);
                    packageRegistrationRepo.CommitChanges();
                }
                tx.Complete();
            }

            InvalidateCache(package.PackageRegistration);
            Cache.InvalidateCacheItem(string.Format("dependentpackages-{0}", package.Key));
        }

        public virtual PackageRegistration FindPackageRegistrationById(string id)
        {
            return FindPackageRegistrationById(id, useCache: true);
        }

        public PackageRegistration FindPackageRegistrationById(string id, bool useCache)
        {
            if (useCache)
            {
                return Cache.Get(string.Format("packageregistration-{0}", id.to_lower()),
                 DateTime.Now.AddMinutes(Cache.DEFAULT_CACHE_TIME_MINUTES),
                 () => packageRegistrationRepo.GetAll()
                        .Include(pr => pr.Owners)
                        .Include(pr => pr.Packages)
                        .Where(pr => pr.Id == id)
                        .SingleOrDefault());
            } 

            return packageRegistrationRepo.GetAll()
                    .Include(pr => pr.Owners)
                    .Include(pr => pr.Packages)
                    .Where(pr => pr.Id == id)
                    .SingleOrDefault();
         
        }

        public virtual Package FindPackageByIdAndVersion(string id, string version, bool allowPrerelease = true)
        {
            return FindPackageByIdAndVersion(id, version, allowPrerelease, useCache: true);
        }

        public virtual Package FindPackageByIdAndVersion(string id, string version, bool allowPrerelease, bool useCache = true)
        {
            if (String.IsNullOrWhiteSpace(id)) throw new ArgumentNullException("id");

            // Optimization: Everytime we look at a package we almost always want to see 
            // all the other packages with the same ID via the PackageRegistration property. 
            // This resulted in a gnarly query. 
            // Instead, we can always query for all packages with the ID.

            IEnumerable<Package> packagesQuery = packageRepo.GetAll()
                                                            .Include(p => p.Authors)
                                                            .Include(p => p.PackageRegistration)
                                                            .Include(p => p.PackageRegistration.Owners)
                                                            .Include(p => p.Files)
                                                            .Include(p => p.Dependencies)
                                                            .Include(p => p.SupportedFrameworks)
                                                            .Where(p => (p.PackageRegistration.Id == id));
            
            var packageVersions = useCache
                            ? Cache.Get(
                                string.Format("packageVersions-{0}", id.to_lower()),
                                DateTime.Now.AddMinutes(Cache.DEFAULT_CACHE_TIME_MINUTES),
                                () => packagesQuery.ToList())
                            : packagesQuery.ToList();

            if (String.IsNullOrEmpty(version) && !allowPrerelease)
            {
                // If there's a specific version given, don't bother filtering by prerelease. You could be asking for a prerelease package.
                packageVersions = packageVersions.Where(p => !p.IsPrerelease).ToList();
            }
            
            Package package = null;
            if (version == null)
            {
                if (allowPrerelease) package = packageVersions.FirstOrDefault(p => p.IsLatest);
                else package = packageVersions.FirstOrDefault(p => p.IsLatestStable);

                // If we couldn't find a package marked as latest, then
                // return the most recent one.
                if (package == null) package = packageVersions.OrderByDescending(p => p.Version).FirstOrDefault();
            } else
            {
                package = packageVersions
                    .Where(
                        p =>
                        p.PackageRegistration.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
                        && p.Version.Equals(version, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
            }

            return package;
        }

        public IEnumerable<Package> GetPackagesForListing(bool includePrerelease)
        {
            IQueryable<Package> packages = null;

            // this is based on what is necessary for search. See Extensions.Search and the searchCriteria
            packages = packageRepo.GetAll()
                                  .Include(p => p.Authors)
                                  .Include(p => p.PackageRegistration)
                                  .Include(p => p.PackageRegistration.Owners)
                                  .Where(p => p.Listed);

            return Cache.Get(string.Format("packageVersions-{0}", includePrerelease),
                    DateTime.Now.AddMinutes(Cache.DEFAULT_CACHE_TIME_MINUTES),
                    () => includePrerelease
                        ? packages.Where(p => p.IsLatest).ToList() //.Distinct(new PackageListingDistinctItemComparer())
                        : packages.Where(p => p.IsLatestStable).ToList() //.Distinct(new PackageListingDistinctItemComparer())
                   );
        }

        //class PackageListingDistinctItemComparer : IEqualityComparer<Package>
        //{
        //    public bool Equals(Package x, Package y)
        //    {
        //        return x.PackageRegistration.Id == y.PackageRegistration.Id;
        //    }

        //    public int GetHashCode(Package obj)
        //    {
        //        return obj.PackageRegistration.Id.GetHashCode();
        //    }
        //}

        public IQueryable<Package> GetSubmittedPackages()
        {
            return packageRepo.GetAll()
                              .Include(x => x.PackageRegistration)
                              .Include(x => x.PackageRegistration.Owners)
                              .Where(p => !p.IsPrerelease)
                              .Where(p => p.StatusForDatabase == submittedStatus);
        }

        public IEnumerable<Package> FindPackagesByOwner(User user)
        {
            return Cache.Get(string.Format("maintainerpackages-{0}", user.Username),
                    DateTime.Now.AddMinutes(Cache.DEFAULT_CACHE_TIME_MINUTES),
                    () => (from pr in packageRegistrationRepo.GetAll()
                           from u in pr.Owners
                           where u.Username == user.Username
                           from p in pr.Packages
                           select p).Include(p => p.PackageRegistration).ToList());

            //return (from pr in packageRegistrationRepo.GetAll()
            //        from u in pr.Owners
            //        where u.Username == user.Username
            //        from p in pr.Packages
            //        select p).Include(p => p.PackageRegistration).ToList();
        }

        public IEnumerable<Package> FindDependentPackages(Package package)
        {
            // Grab all candidates
            var candidateDependents = Cache.Get(string.Format("dependentpackages-{0}", package.Key),
                   DateTime.Now.AddMinutes(Cache.DEFAULT_CACHE_TIME_MINUTES),
                   () => (from p in packageRepo.GetAll()
                          from d in p.Dependencies
                          where d.Id == package.PackageRegistration.Id
                          select d).Include(pk => pk.Package.PackageRegistration).ToList());

            //var candidateDependents = (from p in packageRepo.GetAll()
            //                           from d in p.Dependencies
            //                           where d.Id == package.PackageRegistration.Id
            //                           select d).Include(pk => pk.Package.PackageRegistration).ToList();


            // Now filter by version range.
            var packageVersion = new SemanticVersion(package.Version);
            var dependents = from d in candidateDependents
                             where VersionUtility.ParseVersionSpec(d.VersionSpec).Satisfies(packageVersion)
                             select d;

            return dependents.Select(d => d.Package);
        }

        public void PublishPackage(string id, string version)
        {
            var package = FindPackageByIdAndVersion(id, version, allowPrerelease:true, useCache:false);

            if (package == null) throw new EntityException(Strings.PackageWithIdAndVersionNotFound, id, version);

            MarkPackageListed(package);
        }

        public void AddDownloadStatistics(Package package, string userHostAddress, string userAgent)
        {
            using (MiniProfiler.Current.Step("Updating package stats"))
            {
                packageStatsRepo.InsertOnCommit(
                    new PackageStatistics
                    {
                        // IMPORTANT: Timestamp is managed by the database.
                        IPAddress = userHostAddress,
                        UserAgent = userAgent,
                        PackageKey = package.Key
                    });

                packageStatsRepo.CommitChanges();
            }
        }

        private PackageRegistration CreateOrGetPackageRegistration(User currentUser, IPackage nugetPackage)
        {
            var packageRegistration = FindPackageRegistrationById(nugetPackage.Id, useCache:false);

            if (packageRegistration != null && !packageRegistration.Owners.Contains(currentUser)) throw new EntityException(Strings.PackageIdNotAvailable, nugetPackage.Id);

            if (packageRegistration == null)
            {
                packageRegistration = new PackageRegistration
                {
                    Id = nugetPackage.Id
                };

                packageRegistration.Owners.Add(currentUser);

                packageRegistrationRepo.InsertOnCommit(packageRegistration);
            }

            return packageRegistration;
        }

        private Package CreatePackageFromNuGetPackage(PackageRegistration packageRegistration, IPackage nugetPackage)
        {
            var package = FindPackageByIdAndVersion(packageRegistration.Id, nugetPackage.Version.ToString(), allowPrerelease:true, useCache:false);

            if (package != null)
            {
                switch (package.Status)
                {
                    case PackageStatusType.Rejected :
                        throw new EntityException(
                            string.Format(
                                "This package has been {0} and can no longer be submitted.",
                                package.Status.GetDescriptionOrValue().ToLower()));
                    case PackageStatusType.Submitted :
                        //continue on 
                        break;
                    default :
                        throw new EntityException(
                            "A package with identifier '{0}' and version '{1}' already exists.",
                            packageRegistration.Id,
                            package.Version);
                }
            }

            var now = DateTime.UtcNow;
            var packageFileStream = nugetPackage.GetStream();

            //if new package versus updating an existing package.
            if (package == null) package = new Package();

            package.Version = nugetPackage.Version.ToString();
            package.Description = nugetPackage.Description;
            package.ReleaseNotes = nugetPackage.ReleaseNotes;
            package.RequiresLicenseAcceptance = nugetPackage.RequireLicenseAcceptance;
            package.HashAlgorithm = Constants.Sha512HashAlgorithmId;
            package.Hash = cryptoSvc.GenerateHash(packageFileStream.ReadAllBytes());
            package.PackageFileSize = packageFileStream.Length;
            package.Created = now;
            package.Language = nugetPackage.Language;
            package.LastUpdated = now;
            package.Published = now;
            package.Copyright = nugetPackage.Copyright;
            package.IsPrerelease = !nugetPackage.IsReleaseVersion();
            package.Listed = false;
            package.Status = PackageStatusType.Submitted;
            package.SubmittedStatus = PackageSubmittedStatusType.Ready;
            package.ApprovedDate = null;

            if (package.ReviewedDate.HasValue) package.SubmittedStatus = PackageSubmittedStatusType.Updated;

            //we don't moderate prereleases
            if (package.IsPrerelease)
            {
                package.Listed = true;
                package.Status = PackageStatusType.Exempted;
            }
            if (packageRegistration.IsTrusted)
            {
                package.Listed = true;
                package.Status = PackageStatusType.Approved;
                package.ReviewedDate = now;
                package.ApprovedDate = now;
            }

            package.IconUrl = nugetPackage.IconUrl == null ? string.Empty : nugetPackage.IconUrl.ToString();
            package.LicenseUrl = nugetPackage.LicenseUrl == null ? string.Empty : nugetPackage.LicenseUrl.ToString();
            package.ProjectUrl = nugetPackage.ProjectUrl == null ? string.Empty : nugetPackage.ProjectUrl.ToString();

            package.ProjectSourceUrl = nugetPackage.ProjectSourceUrl == null
                                           ? string.Empty
                                           : nugetPackage.ProjectSourceUrl.ToString();
            package.PackageSourceUrl = nugetPackage.PackageSourceUrl == null
                                           ? string.Empty
                                           : nugetPackage.PackageSourceUrl.ToString();
            package.DocsUrl = nugetPackage.DocsUrl == null ? string.Empty : nugetPackage.DocsUrl.ToString();
            package.MailingListUrl = nugetPackage.MailingListUrl == null
                                         ? string.Empty
                                         : nugetPackage.MailingListUrl.ToString();
            package.BugTrackerUrl = nugetPackage.BugTrackerUrl == null ? string.Empty : nugetPackage.BugTrackerUrl.ToString();
            package.Summary = nugetPackage.Summary ?? string.Empty;
            package.Tags = nugetPackage.Tags ?? string.Empty;
            package.Title = nugetPackage.Title ?? string.Empty;

            foreach (var item in package.Authors.OrEmptyListIfNull().ToList())
            {
                packageAuthorRepo.DeleteOnCommit(item);
            }
            packageAuthorRepo.CommitChanges();
            foreach (var author in nugetPackage.Authors)
            {
                package.Authors.Add(
                    new PackageAuthor
                    {
                        Name = author
                    });
            }

            foreach (var item in package.SupportedFrameworks.OrEmptyListIfNull().ToList())
            {
                packageFrameworksRepo.DeleteOnCommit(item);
            }
            packageFrameworksRepo.CommitChanges();
            var supportedFrameworks = GetSupportedFrameworks(nugetPackage).Select(fn => fn.ToShortNameOrNull()).ToArray();
            if (!supportedFrameworks.AnySafe(sf => sf == null))
            {
                foreach (var supportedFramework in supportedFrameworks)
                {
                    package.SupportedFrameworks.Add(
                        new PackageFramework
                        {
                            TargetFramework = supportedFramework
                        });
                }
            }

            foreach (var item in package.Dependencies.OrEmptyListIfNull().ToList())
            {
                packageDependenciesRepo.DeleteOnCommit(item);
            }
            packageDependenciesRepo.CommitChanges();
            foreach (var dependencySet in nugetPackage.DependencySets)
            {
                if (dependencySet.Dependencies.Count == 0)
                {
                    package.Dependencies.Add(
                        new PackageDependency
                        {
                            Id = null,
                            VersionSpec = null,
                            TargetFramework = dependencySet.TargetFramework.ToShortNameOrNull()
                        });
                } else
                {
                    foreach (var dependency in dependencySet.Dependencies.Select(
                        d => new
                        {
                            d.Id,
                            d.VersionSpec,
                            dependencySet.TargetFramework
                        }))
                    {
                        package.Dependencies.Add(
                            new PackageDependency
                            {
                                Id = dependency.Id,
                                VersionSpec = dependency.VersionSpec == null ? null : dependency.VersionSpec.ToString(),
                                TargetFramework = dependency.TargetFramework.ToShortNameOrNull()
                            });
                    }
                }
            }

            foreach (var item in package.Files.OrEmptyListIfNull().ToList())
            {
                packageFilesRepo.DeleteOnCommit(item);
            }
            packageFilesRepo.CommitChanges();
            foreach (var packageFile in nugetPackage.GetFiles().OrEmptyListIfNull())
            {
                var filePath = packageFile.Path;
                var fileContent = " ";

                IList<string> extensions = new List<string>();
                var approvedExtensions = Configuration.ReadAppSettings("PackageFileTextExtensions");
                if (!string.IsNullOrWhiteSpace(approvedExtensions))
                {
                    foreach (var extension in approvedExtensions.Split(',', ';'))
                    {
                        extensions.Add("." + extension);
                    }
                }
                IList<string> checksumExtensions = new List<string>();
                var checksumApprovedExtensions = Configuration.ReadAppSettings("PackageFileChecksumExtensions");
                if (!string.IsNullOrWhiteSpace(checksumApprovedExtensions))
                {
                    foreach (var extension in checksumApprovedExtensions.Split(',', ';'))
                    {
                        checksumExtensions.Add("." + extension);
                    }
                }

                try
                {
                    var extension = Path.GetExtension(filePath);
                    if (extension != null)
                    {
                        if (extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase)) fileContent = packageFile.GetStream().ReadToEnd();
                        else if (checksumExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                        {
                            var bytes = packageFile.GetStream().ReadAllBytes();
                            var md5Hash =
                                BitConverter.ToString(Convert.FromBase64String(cryptoSvc.GenerateHash(bytes, "MD5")))
                                            .Replace("-", string.Empty);
                            var sha1Hash =
                                BitConverter.ToString(Convert.FromBase64String(cryptoSvc.GenerateHash(bytes, "SHA1")))
                                            .Replace("-", string.Empty);

                            fileContent = string.Format("md5: {0} | sha1: {1}", md5Hash, sha1Hash);
                        }
                    }
                } catch (Exception ex)
                {
                    // Log but swallow the exception
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }

                package.Files.Add(
                    new PackageFile
                    {
                        FilePath = filePath,
                        FileContent = fileContent,
                    });
            }

            package.FlattenedAuthors = package.Authors.Flatten();
            package.FlattenedDependencies = package.Dependencies.Flatten();

            return package;
        }

        public virtual IEnumerable<FrameworkName> GetSupportedFrameworks(IPackage package)
        {
            return package.GetSupportedFrameworks();
        }

        private static void ValidateNuGetPackage(IPackage nugetPackage)
        {
            // TODO: Change this to use DataAnnotations
            if (nugetPackage.Id.Length > 128) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Id", "128");
            if (nugetPackage.Authors != null && String.Join(",", nugetPackage.Authors.ToArray()).Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Authors", "4000");
            if (nugetPackage.Copyright != null && nugetPackage.Copyright.Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Copyright", "4000");
            if (nugetPackage.Description != null && nugetPackage.Description.Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Description", "4000");
            if (nugetPackage.IconUrl != null && nugetPackage.IconUrl.ToString().Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "IconUrl", "4000");
            if (nugetPackage.LicenseUrl != null && nugetPackage.LicenseUrl.ToString().Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "LicenseUrl", "4000");
            if (nugetPackage.ProjectUrl != null && nugetPackage.ProjectUrl.ToString().Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "ProjectUrl", "4000");
            if (nugetPackage.Summary != null && nugetPackage.Summary.Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Summary", "4000");
            if (nugetPackage.Tags != null && nugetPackage.Tags.Length > 4000) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Tags", "4000");
            if (nugetPackage.Title != null && nugetPackage.Title.Length > 256) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Title", "256");

            if (nugetPackage.Version != null && nugetPackage.Version.ToString().Length > 64) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Version", "64");

            if (nugetPackage.Language != null && nugetPackage.Language.Length > 20) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Language", "20");

            foreach (var dependency in nugetPackage.DependencySets.SelectMany(s => s.Dependencies))
            {
                if (dependency.Id != null && dependency.Id.Length > 128) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Dependency.Id", "128");

                if (dependency.VersionSpec != null && dependency.VersionSpec.ToString().Length > 256) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Dependency.VersionSpec", "256");
            }

            if (nugetPackage.DependencySets != null && nugetPackage.DependencySets.Flatten().Length > Int16.MaxValue) throw new EntityException(Strings.NuGetPackagePropertyTooLong, "Dependencies", Int16.MaxValue);
        }

        private static void UpdateIsLatest(PackageRegistration packageRegistration)
        {
            if (!packageRegistration.Packages.Any()) return;

            // TODO: improve setting the latest bit; this is horrible. Trigger maybe? 
            foreach (var pv in packageRegistration.Packages.Where(p => p.IsLatest || p.IsLatestStable))
            {
                pv.IsLatest = false;
                pv.IsLatestStable = false;
                pv.LastUpdated = DateTime.UtcNow;
            }

            // If the last listed package was just unlisted, then we won't find another one
            var latestPackage = FindPackage(packageRegistration.Packages, p => p.Listed);

            if (latestPackage != null)
            {
                latestPackage.IsLatest = true;
                latestPackage.LastUpdated = DateTime.UtcNow;

                if (latestPackage.IsPrerelease)
                {
                    // If the newest uploaded package is a prerelease package, we need to find an older package that is 
                    // a release version and set it to IsLatest.
                    var latestReleasePackage =
                        FindPackage(packageRegistration.Packages.Where(p => !p.IsPrerelease && p.Listed));
                    if (latestReleasePackage != null)
                    {
                        // We could have no release packages
                        latestReleasePackage.IsLatestStable = true;
                        latestReleasePackage.LastUpdated = DateTime.UtcNow;
                    }
                } else
                {
                    // Only release versions are marked as IsLatestStable. 
                    latestPackage.IsLatestStable = true;
                }
            }
        }

        public void AddPackageOwner(PackageRegistration package, User user)
        {
            package.Owners.Add(user);
            packageRepo.CommitChanges();
            

            var request = FindExistingPackageOwnerRequest(package, user);
            if (request != null)
            {
                packageOwnerRequestRepository.DeleteOnCommit(request);
                packageOwnerRequestRepository.CommitChanges();
            }
            Cache.InvalidateCacheItem(string.Format("maintainerpackages-{0}", user.Username));
            InvalidateCache(package);
        }

        public void RemovePackageOwner(PackageRegistration package, User user)
        {
            var pendingOwner = FindExistingPackageOwnerRequest(package, user);
            if (pendingOwner != null)
            {
                packageOwnerRequestRepository.DeleteOnCommit(pendingOwner);
                packageOwnerRequestRepository.CommitChanges();

                Cache.InvalidateCacheItem(string.Format("maintainerpackages-{0}", user.Username));
                InvalidateCache(package);

                return;
            }

            package.Owners.Remove(user);
            packageRepo.CommitChanges();
            Cache.InvalidateCacheItem(string.Format("maintainerpackages-{0}", user.Username));
            InvalidateCache(package);
        }

        // TODO: Should probably be run in a transaction
        public void MarkPackageListed(Package package)
        {
            if (package == null) throw new ArgumentNullException("package");
            if (package.Listed) return;

            if (!package.Listed && (package.IsLatestStable || package.IsLatest)) throw new InvalidOperationException("An unlisted package should never be latest or latest stable!");

            if (package.Status == PackageStatusType.Approved || package.Status == PackageStatusType.Exempted)
            {
                package.Listed = true;
                package.LastUpdated = DateTime.UtcNow;
            }

            UpdateIsLatest(package.PackageRegistration);

            packageRepo.CommitChanges();
            InvalidateCache(package.PackageRegistration);
        }

        public void ChangePackageStatus(Package package, PackageStatusType status, string comments, User user, bool sendEmail)
        {
            if (package.Status == status && package.ReviewComments == comments) return;

            var now = DateTime.UtcNow;

            if (package.Status == PackageStatusType.Submitted) package.SubmittedStatus = PackageSubmittedStatusType.Waiting;

            if (package.Status != status && status != PackageStatusType.Unknown)
            {
                package.Status = status;
                package.ApprovedDate = null;
                package.LastUpdated = now;

                switch (package.Status)
                {
                    case PackageStatusType.Submitted :
                    case PackageStatusType.Rejected :
                        package.Listed = false;
                        break;
                    case PackageStatusType.Approved :
                        package.ApprovedDate = now;
                        package.Listed = true;
                        break;
                    case PackageStatusType.Exempted :
                        package.Listed = true;
                        break;
                }

                UpdateIsLatest(package.PackageRegistration);
            }

            package.ReviewedDate = now;
            package.ReviewedById = user.Key;

            string emailComments = string.Empty;
            if (package.ReviewComments != comments && comments != null)
            {
                package.ReviewComments = comments;
                emailComments = comments;
            }

            packageRepo.CommitChanges();
            if (sendEmail) messageSvc.SendPackageModerationEmail(package, emailComments);

            InvalidateCache(package.PackageRegistration);
        }

        public void ChangeTrustedStatus(Package package, bool trustedPackage, User user)
        {
            if (package.PackageRegistration.IsTrusted == trustedPackage) return;

            package.PackageRegistration.IsTrusted = trustedPackage;
            package.PackageRegistration.TrustedById = user.Key;
            package.PackageRegistration.TrustedDate = DateTime.UtcNow;

            if (trustedPackage)
            {
                var packagesToUpdate =
                    package.PackageRegistration.Packages.Where(
                        p => p.Status == PackageStatusType.Unknown || p.Status == PackageStatusType.Submitted).ToList();

                if (packagesToUpdate.Count != 0)
                {
                    var now = DateTime.UtcNow;
                    foreach (var trustedPkg in packagesToUpdate.OrEmptyListIfNull())
                    {
                        if (trustedPkg.Status == PackageStatusType.Submitted) trustedPkg.Listed = true;

                        trustedPkg.Status = PackageStatusType.Approved;
                        trustedPkg.LastUpdated = now;
                        trustedPkg.ReviewedDate = now;
                        trustedPkg.ApprovedDate = now;
                    }

                    packageRegistrationRepo.CommitChanges();
                }
            }

            UpdateIsLatest(package.PackageRegistration);

            packageRepo.CommitChanges();
        }

        // TODO: Should probably be run in a transaction
        public void MarkPackageUnlisted(Package package)
        {
            if (package == null) throw new ArgumentNullException("package");
            if (!package.Listed) return;

            package.Listed = false;
            package.LastUpdated = DateTime.UtcNow;

            if (package.IsLatest || package.IsLatestStable) UpdateIsLatest(package.PackageRegistration);
            packageRepo.CommitChanges();
            InvalidateCache(package.PackageRegistration);
        }

        private static Package FindPackage(IEnumerable<Package> packages, Func<Package, bool> predicate = null)
        {
            if (predicate != null) packages = packages.Where(predicate);
            SemanticVersion version = packages.Max(p => new SemanticVersion(p.Version));

            if (version == null) return null;
            return packages.First(pv => pv.Version.Equals(version.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        public PackageOwnerRequest CreatePackageOwnerRequest(PackageRegistration package, User currentOwner, User newOwner)
        {
            var existingRequest = FindExistingPackageOwnerRequest(package, newOwner);
            if (existingRequest != null) return existingRequest;

            var newRequest = new PackageOwnerRequest
            {
                PackageRegistrationKey = package.Key,
                RequestingOwnerKey = currentOwner.Key,
                NewOwnerKey = newOwner.Key,
                ConfirmationCode = cryptoSvc.GenerateToken(),
                RequestDate = DateTime.UtcNow
            };

            packageOwnerRequestRepository.InsertOnCommit(newRequest);
            packageOwnerRequestRepository.CommitChanges();
            InvalidateCache(package);
            Cache.InvalidateCacheItem(string.Format("maintainerpackages-{0}", newOwner.Username));

            return newRequest;
        }

        public bool ConfirmPackageOwner(PackageRegistration package, User pendingOwner, string token)
        {
            if (package == null) throw new ArgumentNullException("package");

            if (pendingOwner == null) throw new ArgumentNullException("pendingOwner");

            if (String.IsNullOrEmpty(token)) throw new ArgumentNullException("token");

            if (package.IsOwner(pendingOwner)) return true;

            var request = FindExistingPackageOwnerRequest(package, pendingOwner);
            if (request != null && request.ConfirmationCode == token)
            {
                AddPackageOwner(package, pendingOwner);
                return true;
            }

            return false;
        }

        private PackageOwnerRequest FindExistingPackageOwnerRequest(PackageRegistration package, User pendingOwner)
        {
            return (from request in packageOwnerRequestRepository.GetAll()
                    where request.PackageRegistrationKey == package.Key && request.NewOwnerKey == pendingOwner.Key
                    select request).FirstOrDefault();
        }

        private void InvalidateCache(PackageRegistration package)
        {
            Cache.InvalidateCacheItem(string.Format("packageregistration-{0}", package.Id.to_lower()));
            Cache.InvalidateCacheItem(string.Format("V2Feed-FindPackagesById-{0}", package.Id.to_lower()));
            Cache.InvalidateCacheItem(string.Format("V2Feed-Search-{0}", package.Id.to_lower()));
            Cache.InvalidateCacheItem(string.Format("packageVersions-{0}", package.Id.to_lower()));
            Cache.InvalidateCacheItem("packageVersions-True");
            Cache.InvalidateCacheItem("packageVersions-False");
            Cache.InvalidateCacheItem(string.Format("item-{0}-{1}", typeof(Package).Name, package.Key));
        }

        private void NotifyForModeration(Package package, string comments)
        {
            messageSvc.SendPackageModerationEmail(package, comments);
        }
    }
}
