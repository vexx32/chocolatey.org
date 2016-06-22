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
using System.Data.Entity.Migrations;
using System.Linq;

namespace NuGetGallery.Migrations
{
    public class MigrationsConfiguration : DbMigrationsConfiguration<EntitiesContext>
    {
        public MigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EntitiesContext context)
        {
            var roles = context.Set<Role>();
            if (!roles.Any(x => x.Name == Constants.AdminRoleName))
            {
                roles.Add(
                    new Role
                    {
                        Name = Constants.AdminRoleName
                    });
                context.SaveChanges();
            }
            if (!roles.Any(x => x.Name == Constants.ModeratorsRoleName))
            {
                roles.Add(
                    new Role
                    {
                        Name = Constants.ModeratorsRoleName
                    });
                context.SaveChanges();
            }
            if (!roles.Any(x => x.Name == Constants.ReviewersRoleName))
            {
                roles.Add(
                    new Role
                    {
                        Name = Constants.ReviewersRoleName
                    });
                context.SaveChanges();
            }

            var gallerySettings = context.Set<GallerySetting>();
            if (!gallerySettings.Any())
            {
                gallerySettings.Add(
                    new GallerySetting
                    {
                        SmtpHost = "",
                        SmtpPort = 25,
                        GalleryOwnerEmail = "nobody@nowhere.com",
                        GalleryOwnerName = "Chocolatey Gallery - Local",
                        ConfirmEmailAddresses = false
                    });
                context.SaveChanges();
            } 
        }
    }
}
