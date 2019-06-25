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

namespace NuGetGallery
{
    public class CourseProfileViewModel
    {
        public CourseProfileViewModel(CourseProfile courseProfile)
        {
            Key = courseProfile.Key;
            UserKey = courseProfile.UserKey;
            User User = courseProfile.User;
            Course Course = courseProfile.Course;
            CourseKey = courseProfile.CourseKey;
            Completed = courseProfile.Completed;
            CompletedDate = courseProfile.CompletedDate;

            CourseModuleAchievements = new List<CourseProfileModuleViewModel>();
            foreach (var moduleAchievement in courseProfile.CourseModuleAchievements.OrEmptyListIfNull())
            {
                CourseModuleAchievements.Add(new CourseProfileModuleViewModel(moduleAchievement));
            }
        }

        public int Key { get; set; }
        public int UserKey { get; set; }
        public User User { get; set; }
        public Course Course { get; set; }
        public int CourseKey { get; set; } // foreign key to Course
        public ICollection<CourseProfileModuleViewModel> CourseModuleAchievements { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
