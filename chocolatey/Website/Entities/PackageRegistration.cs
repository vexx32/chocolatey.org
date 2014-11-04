using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NuGetGallery
{
    using System;

    public class PackageRegistration : IEntity
    {
        public PackageRegistration()
        {
            Owners = new HashSet<User>();
            Packages = new HashSet<Package>();
        }

        public int Key { get; set; }

        [StringLength(128), Required]
        public string Id { get; set; }
        public int DownloadCount { get; set; }
        public virtual ICollection<User> Owners { get; set; }
        public virtual ICollection<Package> Packages { get; set; }
        public bool IsTrusted { get; set; }
        public DateTime? TrustedDate { get; set; }
        public virtual User TrustedBy { get; set; }
        public int? TrustedById { get; set; } 
    }
}