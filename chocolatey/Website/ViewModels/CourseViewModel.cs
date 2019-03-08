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

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace NuGetGallery
{
    public class CourseViewModel
    {
        [StringLength(255)]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[.\S]+\@[.\S]+\.[.\S]+", ErrorMessage = "This doesn't appear to be a valid email address.")]
        public string EmailAddress { get; set; }

        public string PendingNewEmailAddress { get; set; }

        [Display(Name = "Receive Email Notifications")]
        public bool EmailAllowed { get; set; }     
        
        [Display(Name = "Receive Email For All Moderation-Related Notifications")]
        public bool EmailAllModerationNotifications { get; set; }

        public string Username { get; set; }

        [Display(Name = "Question One")]
        public string QuestOne { get; set; }

        [Display(Name = "Question Two")]
        public string QuestTwo { get; set; }

        [Display(Name = "Question Three")]
        public string QuestThree { get; set; }

        [Display(Name = "Question Four")]
        public string QuestFour { get; set; }

        [Display(Name = "Course Name")]
        public string CourseName { get; set; }

        public bool CompletedCourse { get; set; }
  
        public bool CompletedModOne { get; set; }

        public bool CompletedModTwo { get; set; }

        public bool CompletedModThree { get; set; }

        public bool CompletedModFour { get; set; }

        public bool CompletedModFive { get; set; }

        public bool CompletedModSix { get; set; }

        public bool CompletedModSeven { get; set; }

        public bool CompletedModEight { get; set; }

        public bool CompletedModNine { get; set; }

        public bool CompletedModTen { get; set; }

        public bool CompletedModEleven { get; set; }

        public bool CompletedModTwelve { get; set; }

        public ICollection<CourseProfileViewModel> UserCourseProfiles { get; set; }
    }
}
