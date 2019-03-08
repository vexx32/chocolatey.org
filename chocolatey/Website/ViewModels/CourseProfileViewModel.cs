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

namespace NuGetGallery
{
    public class CourseProfileViewModel
    {
        public CourseProfileViewModel(CourseProfile siteProfile)
        {
            Name = siteProfile.Name;
            Url = siteProfile.Url;
            Image = siteProfile.Image;
            Completed = siteProfile.Completed;
            ModOne = siteProfile.ModOne;
            ModTwo = siteProfile.ModTwo;
            ModThree = siteProfile.ModThree;
            ModFour = siteProfile.ModFour;
            ModFive = siteProfile.ModFive;
            ModSix = siteProfile.ModSix;
            ModSeven = siteProfile.ModSeven;
            ModEight = siteProfile.ModEight;
            ModNine = siteProfile.ModNine;
            ModTen = siteProfile.ModTen;
            ModEleven = siteProfile.ModEleven;
            ModTwelve = siteProfile.ModTwelve;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public bool Completed { get; set; }
        public bool ModOne { get; set; }
        public bool ModTwo { get; set; }
        public bool ModThree { get; set; }
        public bool ModFour { get; set; }
        public bool ModFive { get; set; }
        public bool ModSix { get; set; }
        public bool ModSeven { get; set; }
        public bool ModEight { get; set; }
        public bool ModNine { get; set; }
        public bool ModTen { get; set; }
        public bool ModEleven { get; set; }
        public bool ModTwelve { get; set; }
    }
}
