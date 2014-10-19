using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NuGetGallery
{
    [Bind(Include="Message,CopySender,Email,ConfirmedUser")]
    public class ContactOwnersViewModel
    {
        public string PackageId { get; set; }

        public IEnumerable<User> Owners { get; set; }

        [Display(Name = "Send me a copy")]
        public bool CopySender { get; set; }

        [Required(ErrorMessage = "Please enter a message.")]
        [StringLength(4000)]
        public string Message { get; set; }

        [Required(ErrorMessage = "Please enter your email address.")]
        [StringLength(4000)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"(?i)^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$", ErrorMessage = "This doesn't appear to be a valid email address.")]
        public string Email { get; set; }

        public bool ConfirmedUser { get; set; }

    }
}