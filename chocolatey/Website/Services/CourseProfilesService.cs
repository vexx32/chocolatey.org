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
using System.Linq;

namespace NuGetGallery
{
    public class CourseProfilesService : ICourseProfilesService
    {
        private readonly IEntityRepository<CourseProfile> profileRepo;

        public CourseProfilesService(IEntityRepository<CourseProfile> profileRepo)
        {
            this.profileRepo = profileRepo;
        }

        public IEnumerable<CourseProfile> GetUserProfiles(User user)
        {
            return profileRepo.GetAll().Where(x => x.Username == user.Username).ToList();
        }

        public void SaveProfiles(User user, CourseViewModel profile)
        {
            var siteProfiles = GetUserProfiles(user).AsQueryable();

            CompareAndPrepareProfile(
                profile.CourseName,
                user.Username,
                string.Empty,
                siteProfiles,
                profile.CompletedCourse,
                profile.CompletedModOne,
                profile.CompletedModTwo,
                profile.CompletedModThree,
                profile.CompletedModFour,
                profile.CompletedModFive,
                profile.CompletedModSix,
                profile.CompletedModSeven,
                profile.CompletedModEight,
                profile.CompletedModNine,
                profile.CompletedModTen,
                profile.CompletedModEleven,
                profile.CompletedModTwelve,
                prefix: string.Empty);

            profileRepo.CommitChanges();
        }

        private void CompareAndPrepareProfile(
            string profileName,
            string userName,
            string badgeUrl,
            IQueryable<CourseProfile> siteProfiles,
            bool completed,
            bool modOne,
            bool modTwo,
            bool modThree,
            bool modFour,
            bool modFive,
            bool modSix,
            bool modSeven,
            bool modEight,
            bool modNine,
            bool modTen,
            bool modEleven,
            bool modTwelve,
            string prefix)
        {
            var siteProfile = siteProfiles.FirstOrDefault(x => x.Name == profileName);

            // This deletes the row from database
            //if (siteProfile != null && string.IsNullOrWhiteSpace(profileValue)) profileRepo.DeleteOnCommit(siteProfile);
            
            if (siteProfile == null)
            {
                var newSiteProfile = new CourseProfile();
                newSiteProfile.Username = userName;
                newSiteProfile.Name = profileName;
                newSiteProfile.Url = prefix;
                newSiteProfile.Image = badgeUrl;
                newSiteProfile.Completed = completed;
                newSiteProfile.ModOne = modOne;
                newSiteProfile.ModTwo = modTwo;
                newSiteProfile.ModThree = modThree;
                newSiteProfile.ModFour = modFour;
                newSiteProfile.ModFive = modFive;
                newSiteProfile.ModSix = modSix;
                newSiteProfile.ModSeven = modSeven;
                newSiteProfile.ModEight = modEight;
                newSiteProfile.ModNine = modNine;
                newSiteProfile.ModTen = modTen;
                newSiteProfile.ModEleven = modEleven;
                newSiteProfile.ModTwelve = modTwelve;
                profileRepo.InsertOnCommit(newSiteProfile);
            }

            // Getting Started with Chocolatey- Update Records
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModOne == false)
            {
                siteProfile.ModOne = modOne;
            }
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModTwo == false)
            {
                siteProfile.ModTwo = modTwo;
            }
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModThree == false)
            {
                siteProfile.ModThree = modThree;
            }
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModFour == false)
            {
                siteProfile.ModFour = modFour;
            }
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModFive == false)
            {
                siteProfile.ModFive = modFive;
            }
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModSix == false)
            {
                siteProfile.ModSix = modSix;
            }
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModSeven == false)
            {
                siteProfile.ModSeven = modSeven;
            }
            // Getting Started with Chocolatey- Course Completed
            if (siteProfile != null && siteProfile.Name == CourseProfileConstants.GettingStartedWithChocolatey && siteProfile.ModOne == true && siteProfile.ModTwo == true && siteProfile.ModThree == true && siteProfile.ModFour == true && siteProfile.ModFive == true && siteProfile.ModSix == true && siteProfile.ModSeven == true)
            {
                siteProfile.Completed = true;
                siteProfile.Url = CourseProfileConstants.GettingStartedWithChocolateyUrl;
                siteProfile.Image = CourseProfileConstants.Images.gettingStartedWithChocolatey;
            }
        }
    }

    public static class CourseProfileConstants
    {
        public const string GettingStartedWithChocolatey = "Getting Started with Chocolatey";
        public const string GettingStartedWithChocolateyUrl = "/courses/getting-started";

        public static class Images
        {
            private const string URLPATH = "~/Content/Images/Badges";

            public static string Url(string fileName)
            {
                return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName);
            }

            public static readonly string gettingStartedWithChocolatey = Url("GettingStartedCourse.png");
        }
    }
}
