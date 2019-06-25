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
using System.Linq;

namespace NuGetGallery
{

    public class CourseProfilesService : ICourseProfilesService
    {
        private readonly IEntityRepository<CourseProfile> courseProfileRepo;

        public CourseProfilesService(IEntityRepository<CourseProfile> profileRepo)
        {
            this.courseProfileRepo = profileRepo;
        }

        public IEnumerable<CourseProfile> GetUserCourseProfiles(User user)
        {
            return courseProfileRepo.GetAll().Where(x => x.UserKey == user.Key).ToList();
        }

        public void SaveCourseProfiles(User user, CourseDisplayViewModel courseProfile)
        {
            var existingCourseProfiles = GetUserCourseProfiles(user).AsQueryable();

            CompareAndPrepareCourseProfile(
                courseProfile,
                user,
                existingCourseProfiles);

            courseProfileRepo.CommitChanges();
        }

        private void CompareAndPrepareCourseProfile(
            CourseProfileViewModel courseProfileModel,
            User user,
            IQueryable<CourseProfile> existingCourseProfiles)
        {
            var courseProfile = existingCourseProfiles.FirstOrDefault(x => x.CourseKey == courseProfileModel.CourseKey);
            
            if (courseProfile == null)
            {
                courseProfile = new CourseProfile();
                courseProfile.CourseKey = courseProfileModel.CourseKey;
                courseProfile.UserKey = user.Key;
                courseProfileRepo.InsertOnCommit(courseProfile);
            }

            courseProfile.Completed = courseProfileModel.Completed;
            courseProfile.CompletedDate = courseProfileModel.CompletedDate;

            foreach (CourseProfileModuleViewModel moduleModel in courseProfileModel.CourseModuleAchievements.OrEmptyListIfNull())
            {
                if (moduleModel.IsCompleted) {
                    var module = courseProfile.CourseModuleAchievements.FirstOrDefault(x => x.CourseModuleKey == moduleModel.CourseModuleKey);

                    if (module == null) {
                        courseProfile.CourseModuleAchievements.Add(new CourseProfileModule {
                            CourseProfileKey = courseProfile.Key,
                            CourseModuleKey = moduleModel.Key,
                            CompletedDate = moduleModel.CompletedDate
                        });
                    }
                }
            }
        }
    }

    public static class CourseConstants
    {
        public const string GettingStartedWithChocolatey = "Getting Started with Chocolatey";
        public const string GettingStartedWithChocolateyUrl = "/courses/getting-started";
        public const string InstallingUpgradingUninstalling = "Installing, Upgrading, and Uninstalling Chocolatey";
        public const string InstallingUpgradingUninstallingUrl = "/courses/installation";
        public const string CreatingChocolateyPackages = "Creating Chocolatey Packages";
        public const string CreatingChocolateyPackagesUrl = "/courses/creating-chocolatey-packages";

        public static class BadgeImages
        {
            private const string URLPATH = "~/Content/Images/Badges";

            public static string Url(string fileName)
            {
                return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName);
            }

            public static readonly string GettingStartedWithChocolatey = Url("GettingStartedCourse.png");
            public static readonly string InstallingUpgradingUninstalling = Url("InstallingUpgradingUninstalling.png");
            public static readonly string CreatingChocolateyPackages = Url("CreatingChocolateyPackages.png");
        }
    }
}
