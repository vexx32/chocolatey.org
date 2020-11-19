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
    public class EventViewModel
    {
        public string IsArchived { get; set; }
        public string UrlPath { get; set; }
        public string Type { get; set; }
        public DateTime? EventDate { get; set; }
        public string Time { get; set; }
        public string Duration { get; set; }
        public string Title { get; set; }
        public string Tagline { get; set; }
        public IEnumerable<string> Speakers { get; set; }
        public string Image { get; set; }
        public string RegisterLink { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string Summary { get; set; }
        public string Post { get; set; }
        public string IncludeRegisterPage { get; set; }
        public string IsOnDemand { get; set; }
        public string EventDateRange { get; set; }
    }
}
