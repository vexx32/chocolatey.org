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
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using AnglicanGeek.MarkdownMailer;
using Elmah;

namespace NuGetGallery
{
    public class MessageService : IMessageService
    {
        private readonly IMailSender mailSender;
        private readonly GallerySetting settings;

        public MessageService(IMailSender mailSender, GallerySetting settings)
        {
            this.mailSender = mailSender;
            this.settings = settings;
        }

        private void SendMessage(MailMessage mailMessage, bool copySender = false)
        {
            try
            {
                mailSender.Send(mailMessage);
                if (copySender)
                {
                    var senderNote = string.Format(
                        "You sent the following message via {0}:{1}{1}", settings.GalleryOwnerName, Environment.NewLine);
                    mailMessage.To.Clear();
                    mailMessage.Body = senderNote + mailMessage.Body;
                    mailMessage.To.Add(mailMessage.From);
                    mailSender.Send(mailMessage);
                }
            } catch (SmtpException ex)
            {
                // Log but swallow the exception
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public void ReportAbuse(MailAddress fromAddress, Package package, string message, string packageUrl, bool copySender)
        {
            string subject = "[{0}] Abuse Report for Package '{1}' Version '{2}'";
            string body = @"_User {0} ({1}) reports the package '{2}' version '{3}' as abusive. 
{0} left the following information in the report:_

{4}

_Message sent from {5}_

Current Maintainer(s): {7}
Package Url: {6}
";
            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                fromAddress.DisplayName,
                fromAddress.Address,
                package.PackageRegistration.Id,
                package.Version,
                message,
                settings.GalleryOwnerName,
                packageUrl,
                string.Join(", ", package.PackageRegistration.Owners.Select(x => x.Username)));

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(
                    CultureInfo.CurrentCulture,
                    subject,
                    settings.GalleryOwnerName,
                    package.PackageRegistration.Id,
                    package.Version);
                mailMessage.Body = body;
                mailMessage.From = fromAddress;

                mailMessage.To.Add(settings.GalleryOwnerEmail);
                SendMessage(mailMessage, copySender);
            }
        }

        public void ContactSiteAdmins(
            MailAddress fromAddress, Package package, string message, string packageUrl, bool copySender)
        {
            string subject = "[{0}] Contact Site Admins for Package '{1}' Version '{2}'";
            string body = @"_User {0} ({1}) is reporting on '{2}' version '{3}'. 
{0} left the following information in the report:_

{4}

_Message sent from {5}_

Current Maintainer(s): {7}
Package Url: {6}
";
            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                fromAddress.DisplayName,
                fromAddress.Address,
                package.PackageRegistration.Id,
                package.Version,
                message,
                settings.GalleryOwnerName,
                packageUrl,
                string.Join(", ", package.PackageRegistration.Owners.Select(x => x.Username)));

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(
                    CultureInfo.CurrentCulture,
                    subject,
                    settings.GalleryOwnerName,
                    package.PackageRegistration.Id,
                    package.Version);
                mailMessage.Body = body;
                mailMessage.From = fromAddress;
                mailMessage.To.Add(Configuration.ReadAppSettings("ModeratorEmail"));
                SendMessage(mailMessage, copySender);
            }
        }

        public void ContactUs(MailAddress fromAddress, string contactType, string message)
        {
            string subject = "Chocolatey - Contact Us - {0}".format_with(contactType);
            string body = message;

            var to = Configuration.ReadAppSettings("ContactUsEmail");
            //refactor this a bit
            if (contactType == "Website")
            {
                to = Configuration.ReadAppSettings("ModeratorEmail");
            }

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = fromAddress;
                mailMessage.To.Add(to);
                SendMessage(mailMessage);
            }
        }

        public void SendContactOwnersMessage(
            MailAddress fromAddress,
            PackageRegistration packageRegistration,
            string message,
            string emailSettingsUrl,
            string packageUrl,
            bool copySender)
        {
            string subject = "[{0}] Message for maintainers of the package '{1}'";
            string body = @"_User {0} &lt;{1}&gt; sends the following message to the maintainers of Package '{2}'._

{3}

Package Url: {6}

<b>Pro-tip:</b> It is recommended to (also) post your answer to [the {2} comment section](http://chocolatey.org/packages/{2}#disqus) so that the information is publicly available.
This can prevent you from receiving the same question from multiple users.

Package comments: [http://chocolatey.org/packages/{2}#disqus](http://chocolatey.org/packages/{2}#disqus)

-----------------------------------------------
<em style=""font-size: 0.8em;"">
    To stop receiving contact emails as a maintainer of this package, sign in to the {4} and 
    [change your email notification settings]({5}).
</em>";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                fromAddress.DisplayName,
                fromAddress.Address,
                packageRegistration.Id,
                message,
                settings.GalleryOwnerName,
                emailSettingsUrl,
                packageUrl);

            subject = String.Format(CultureInfo.CurrentCulture, subject, settings.GalleryOwnerName, packageRegistration.Id);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = fromAddress;

                AddOwnersToMailMessage(packageRegistration, mailMessage);
                if (mailMessage.To.Any()) SendMessage(mailMessage, copySender);
            }
        }

        public void SendCommentNotificationToMaintainers(
            PackageRegistration packageRegistration, CommentViewModel comment, string packageUrl)
        {
            string body = @"Comment: {0}

This comment has been added to the disqus forum thread for {1}. 

Package Url: {2}
Comment Url: {3}

{4}
";
            string subject = "[{0}] New disqus comment for maintainers of '{1}'";
            string disqusCommentUrl = string.Format("{0}#comment-{1}", packageUrl, comment.Id);

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                comment.Text,
                packageRegistration.Id,
                packageUrl,
                disqusCommentUrl,
                GetDisqusInformationMessage());

            subject = String.Format(CultureInfo.CurrentCulture, subject, settings.GalleryOwnerName, packageRegistration.Id);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = new MailAddress("chocolatey@noreply.org", "NO REPLY - Chocolatey");

                AddOwnersToMailMessage(packageRegistration, mailMessage);
                if (mailMessage.To.Any()) SendMessage(mailMessage);
            }
        }

        private static void AddOwnersToMailMessage(PackageRegistration packageRegistration, MailMessage mailMessage, bool requireEmail = false)
        {
            foreach (var owner in packageRegistration.Owners.Where(o => o.EmailAllowed || requireEmail))
            {
                mailMessage.To.Add(owner.ToMailAddress());
            }
        }

        public void SendNewAccountEmail(MailAddress toAddress, string confirmationUrl)
        {
            string body = @"Thank you for registering with the {0}. 
We can't wait to see what packages you'll upload.

Remember to read the packaging guidelines:
https://chocolatey.org/docs/create-packages

All packages sumbitted to the Chocolatey Gallery must meet these guidelines.

If you can't adhere to this advice, the chocolatey gods might get unhappy and put you in the hall of shame.
Hall of shame users might notice their packages getting unlisted or deleted.

So we can be sure to contact you, please verify your email address and click the following link:

{1}

Thanks,
The {0} Team";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                settings.GalleryOwnerName,
                confirmationUrl);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(
                    CultureInfo.CurrentCulture, "[{0}] Please verify your account.", settings.GalleryOwnerName);
                mailMessage.Body = body;
                mailMessage.From = new MailAddress(settings.GalleryOwnerEmail, settings.GalleryOwnerName);

                mailMessage.To.Add(toAddress);
                SendMessage(mailMessage);
            }
        }

        public void SendEmailChangeConfirmationNotice(MailAddress newEmailAddress, string confirmationUrl)
        {
            string body = @"You recently changed your {0} email address. 

To verify your new email address, please click the following link:

{1}

Thanks,
The {0} Team";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                settings.GalleryOwnerName,
                confirmationUrl);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(
                    CultureInfo.CurrentCulture, "[{0}] Please verify your new email address.", settings.GalleryOwnerName);
                mailMessage.Body = body;
                mailMessage.From = new MailAddress(settings.GalleryOwnerEmail, settings.GalleryOwnerName);

                mailMessage.To.Add(newEmailAddress);
                SendMessage(mailMessage);
            }
        }

        public void SendEmailChangeNoticeToPreviousEmailAddress(User user, string oldEmailAddress)
        {
            string body = @"Hi there,

The email address associated to your {0} account was recently 
changed from _{1}_ to _{2}_.

**Note:** If you are getting this and both emails are the same, there is likely a space in your email address. 
If you go into your account and set the email address to the same thing, it should remove this message.

Thanks,
The {0} Team";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                settings.GalleryOwnerName,
                oldEmailAddress,
                user.EmailAddress);

            string subject = String.Format(
                CultureInfo.CurrentCulture, "[{0}] Recent changes to your account.", settings.GalleryOwnerName);

            if (user.EmailAddress.ToStringSafe().Trim().ToLower() != oldEmailAddress.ToStringSafe().Trim().ToLower())
            {
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.From = new MailAddress(settings.GalleryOwnerEmail, settings.GalleryOwnerName);

                    mailMessage.To.Add(new MailAddress(oldEmailAddress, user.Username));
                    SendMessage(mailMessage);
                }
            }
        }

        public void SendPasswordResetInstructions(User user, string resetPasswordUrl)
        {
            string body = @"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next {0} hours to reset your password:

{1}

Thanks,
The {2} Team";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                Constants.DefaultPasswordResetTokenExpirationHours,
                resetPasswordUrl,
                settings.GalleryOwnerName);

            string subject = String.Format(
                CultureInfo.CurrentCulture, "[{0}] Please reset your password.", settings.GalleryOwnerName);
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = new MailAddress(settings.GalleryOwnerEmail, settings.GalleryOwnerName);

                mailMessage.To.Add(user.ToMailAddress());
                SendMessage(mailMessage);
            }
        }

        public void SendPackageOwnerRequest(User fromUser, User toUser, PackageRegistration package, string confirmationUrl)
        {
            if (!toUser.EmailAllowed) return;

            string subject = "[{0}] The user '{1}' wants to add you as a maintainer of the package '{2}'.";

            string body = @"The user '{0}' wants to add you as a maintainer of the package '{1}'. 
If you do not want to be listed as a maintainer of this package, simply delete this email.

To accept this request and become a listed maintainer of the package, click the following URL:

{2}

Thanks,
The {3} Team";

            body = String.Format(
                CultureInfo.CurrentCulture, body, fromUser.Username, package.Id, confirmationUrl, settings.GalleryOwnerName);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(
                    CultureInfo.CurrentCulture, subject, settings.GalleryOwnerName, fromUser.Username, package.Id);
                mailMessage.Body = body;
                mailMessage.From = fromUser.ToMailAddress();

                mailMessage.To.Add(toUser.ToMailAddress());
                SendMessage(mailMessage);
            }
        }

        public void SendPackageOwnerConfirmation(User fromUser, User toUser, PackageRegistration package)
        {
            if (!toUser.EmailAllowed) return;
            var packageUrl = string.Format(
                "{0}packages/{1}", EnsureTrailingSlash(Configuration.ReadAppSettings("SiteRoot")), package.Id);

            string subject = "[{0}] The user '{1}' has added you as a maintainer of the package '{2}'.";

            string body = @"The user '{0}' has added you as a maintainer of the package '{1}'. 

Package Url: {2}

Thanks,
The {3} Team";

            body = String.Format(
                CultureInfo.CurrentCulture, body, fromUser.Username, package.Id, packageUrl, settings.GalleryOwnerName);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(
                    CultureInfo.CurrentCulture, subject, settings.GalleryOwnerName, fromUser.Username, package.Id);
                mailMessage.Body = body;
                mailMessage.From = fromUser.ToMailAddress();

                mailMessage.To.Add(toUser.ToMailAddress());
                SendMessage(mailMessage);
            }
        }

        public void SendPackageModerationEmail(Package package, string comments, string subjectComment, User fromUser)
        {
            string subject = "[{0}] {1} v{2} Moderation{3}";
            var packageUrl = string.Format(
                "{0}packages/{1}/{2}",
                EnsureTrailingSlash(Configuration.ReadAppSettings("SiteRoot")),
                package.PackageRegistration.Id,
                package.Version);
            string body = @"'{0}' is {3}.
{6}{4}

Package Url: {1} 
Maintainer(s): {2}
{5}
";
            bool submitted = package.Status == PackageStatusType.Submitted;
            var tldrText = "Current status = ";
            switch (package.SubmittedStatus)
            {
                case PackageSubmittedStatusType.Pending:
                     tldrText += "Pending automated review";
                    break;   
                case PackageSubmittedStatusType.Ready:
                     tldrText += "Ready for review";
                    break;    
                case PackageSubmittedStatusType.Waiting:
                     tldrText += "Waiting for Maintainer to take corrective action";
                    break;
                case PackageSubmittedStatusType.Responded:
                     tldrText += "Maintainer responded, waiting for review/Maintainer update";
                    break;
                case PackageSubmittedStatusType.Updated:
                     tldrText += "Maintainer updated, waiting for Reviewer";
                    break;
            }

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                package.PackageRegistration.Id,
                packageUrl,
                string.Join(", ", package.PackageRegistration.Owners.Select(x => x.Username)),
                package.Status.GetDescriptionOrValue(),
                GetModerationMessage(package, comments, fromUser),
                GetInformationForMaintainers(package, comments),
                submitted ? "{0}{1}{1}".format_with(tldrText,Environment.NewLine) : string.Empty
            );

            subject = String.Format(
                CultureInfo.CurrentCulture,
                subject,
                settings.GalleryOwnerName,
                package.PackageRegistration.Id,
                package.Version,
                !string.IsNullOrWhiteSpace(subjectComment) ? " - " + subjectComment.to_string() : string.Empty
            );
            
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = new MailAddress("chocolatey@noreply.org", "NO REPLY - Chocolatey");

                AddOwnersToMailMessage(package.PackageRegistration, mailMessage, requireEmail: true);
                //mailMessage.To.Add(settings.GalleryOwnerEmail);
                if (mailMessage.To.Any()) SendMessage(mailMessage);
            }
        }

        public void SendPackageModerationReviewerEmail(Package package, string comments, User fromUser)
        {
            string subject = "[{0}] {1} v{2} - Moderation Review Response";
            var packageUrl = string.Format(
                "{0}packages/{1}/{2}",
                EnsureTrailingSlash(Configuration.ReadAppSettings("SiteRoot")),
                package.PackageRegistration.Id,
                package.Version);
            string body = @"'{0}' is {3}{5}.
{4}

Package Url: {1} 
Maintainer(s): {2}
";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                package.PackageRegistration.Id,
                packageUrl,
                string.Join(", ", package.PackageRegistration.Owners.Select(x => x.Username)),
                package.Status.GetDescriptionOrValue(),
                GetModerationMessage(package, comments, fromUser),
                package.Status == PackageStatusType.Approved && !string.IsNullOrWhiteSpace(comments)
                    ? " with comments"
                    : string.Empty);

            subject = String.Format(
                CultureInfo.CurrentCulture,
                subject,
                settings.GalleryOwnerName,
                package.PackageRegistration.Id,
                package.Version);
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = new MailAddress("chocolatey@noreply.org", "NO REPLY - Chocolatey");

                if (package.ReviewedBy != null)
                {
                    mailMessage.To.Add(package.ReviewedBy.EmailAddress);
                }
                
                if (mailMessage.To.Any()) SendMessage(mailMessage);
            }
        }

        public void SendPackageTestFailureMessage(Package package, string resultDetailsUrl)
        {
            string subject = string.Format("[{0}] {1} v{2} - Failed Automated Testing", 
                settings.GalleryOwnerName,
                package.PackageRegistration.Id,
                package.Version);
            var packageUrl = string.Format(
                "{0}packages/{1}/{2}",
                EnsureTrailingSlash(Configuration.ReadAppSettings("SiteRoot")),
                package.PackageRegistration.Id,
                package.Version);
            string body = string.Format(@"{0} (v{4} - likely the current latest version) is failing automatic package install/uninstall testing.

* Automated package testing on the latest version will occur from time to time. 
* If the verifier is [incompatible with the package](https://github.com/chocolatey/package-verifier/wiki), please contact site admins if package needs to bypass testing (e.g. package installs specific drivers).
* **NEW!** We have a [test environment](https://github.com/chocolatey/chocolatey-test-environment) for you to replicate the testing we do. This can be used at any time to test packages! See https://github.com/chocolatey/chocolatey-test-environment
* Automated testing can also fail when a package is not completely silent.

Please see the test results link below for more details.

Package Url: {1} 
Test Results: {2}
Maintainer(s): {3}
",
                package.PackageRegistration.Id,
                packageUrl,
                resultDetailsUrl,
                string.Join(", ", package.PackageRegistration.Owners.Select(x => x.Username)),
                package.Version);
            
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = new MailAddress("chocolatey@noreply.org", "NO REPLY - Chocolatey");

                AddOwnersToMailMessage(package.PackageRegistration, mailMessage, requireEmail: true);
                if (mailMessage.To.Any()) SendMessage(mailMessage);
            }
        }

        private string GetDisqusInformationMessage()
        {
            return @"
## DO NOT REPLY TO THIS MESSAGE!

### Information for Maintainers

 * Disqus comments can be moderated. As a result, you may not see the above comment on the package page until the comment is moderated by a site admin.
 * If the comment is not subject to moderation (no urls or from a commenter with a low rep), you can take action straight away.
 * You will not be able to reply to the Disqus comment until it has been approved by a site admin. Site admins typically review Disqus pending comments about once a week. If you need them to take action sooner, please use the contact site admins link from the package page.
 * You are encouraged to reply directly to the Disqus comment when required action is taken.
";
        }

        private string GetInformationForMaintainers(Package package, string comments)
        {
            if (package.Status == PackageStatusType.Submitted && string.IsNullOrWhiteSpace(comments)) return string.Format(@"
For urgent issues (including packages that are security releases), please reach out to us immediately on Gitter at https://gitter.im/chocolatey/choco 

### Information for Maintainers

* If you have fixes, repush your package with the ***exact*** **same version** (unless the change we requested was based on an incorrect version). This is allowed until approved.
* If you need to update or respond to package review information, please visit your package page and respond there (you may need to login first).  
* If you have questions, please reach out to us at {0}.

#### Other Pertinent Information

 * Moderators typically review a package within about a week or less. Many times you may find it to be faster.
 * If you have not heard anything within a week or two, please respond in the review comments on the package page (login first) and ask for status.
 * If the package is an urgent release (resolves security issues or CVEs), reach out to us immediately on Gitter at https://gitter.im/chocolatey/choco
 * Packages must conform to our guidelines https://chocolatey.org/docs/create-packages
 * Packages typically get rejected for not conforming to our naming guidelines - https://chocolatey.org/docs/create-packages#naming-your-package
",
 Configuration.ReadAppSettings("ModeratorEmail"));
            else if (package.Status == PackageStatusType.Submitted) return @"
#### Maintainer Notes

 * If we've asked you to make changes, repush your updated package with the ***exact*** **_same_ version** (unless the change we requested was based on an incorrect version).";

            return string.Empty;
        }

        private string GetModerationMessage(Package package, string comments, User fromUser)
        {
            var message = new StringBuilder();

            if (package.IsPrerelease && package.SubmittedStatus == PackageSubmittedStatusType.Pending)
            {
                message.AppendFormat("**NOTE**: This version is a prerelease and prerelease versions are exempted from human moderation. However it will go through automated review and will automatically list when it has completed validation and verification (even if they fail). The two are typically finished within 1-2 hours or less after the package has pushed, depending on the verifier queue. We'll send you a message when it is listed. If you haven't received a message within 6 hours, please contact us for further investigation.{0}",Environment.NewLine);
            }
            if (package.PackageRegistration.IsTrusted && package.SubmittedStatus == PackageSubmittedStatusType.Pending)
            {
                message.AppendFormat("**NOTE**: This package is trusted and bypasses human moderation. However it will go through automated review and will automatically list when it has completed validation and verification (even if they fail). The two are typically finished within 1-2 hours or less after the package has pushed, depending on the verifier queue. We'll send you a message when it is listed. If you haven't received a message within 6 hours, please contact us for further investigation.{0}", Environment.NewLine);
                message.AppendFormat("**NOTE**: Starting in March **trusted packages will be held for approval until they pass both validation and verification**. This will give maintainers a chance to fix bad package versions prior to becoming immutable. More details will be provided in an email to all package maintainers sometime in January.{0}", Environment.NewLine);
            }

            if (!string.IsNullOrWhiteSpace(comments))
            {
                // fromUser could be null. If the package hasn't been reviewed yet it will be unless this message is being sent to the reviewer.
                message.AppendFormat(
                    "{0} left the following comment(s):{1}", fromUser != null ? fromUser.Username : "The reviewer", Environment.NewLine);
                message.Append(Environment.NewLine + comments);
            } else if (package.Status == PackageStatusType.Rejected)
            {
                // fromUser will not be null here.
                message.AppendFormat(
                    "{0} left the following comment(s):{1}", fromUser.Username, Environment.NewLine);
                message.Append(Environment.NewLine + package.ReviewComments);
            }

            switch (package.Status)
            {
                case PackageStatusType.Submitted :
                    break;
                case PackageStatusType.Rejected :
                case PackageStatusType.Approved :
                case PackageStatusType.Exempted :
                    message.AppendFormat(
                        "{3}{3}The package was {0} by {1} on {2}.",
                        package.Status.GetDescriptionOrValue().ToLower(),
                        fromUser != null ? fromUser.Username : "the reviewer",
                        package.ReviewedDate.GetValueOrDefault().ToShortDateString(),
                        Environment.NewLine);
                    break;
            }

             message.AppendFormat(@"{0}## Attention - DO NOT REPLY TO THIS MESSAGE!
 * If you need to update or respond to package review information, please login and visit your package page (listed below).
 * You can also **self-reject packages that are no longer relevant**! See [self-reject](https://chocolatey.org/faq#how-do-i-self-reject-a-package) for more information.",
                Environment.NewLine);

            return message.ToString();
        }

        private static string EnsureTrailingSlash(string siteRoot)
        {
            if (string.IsNullOrWhiteSpace(siteRoot)) return string.Empty;
            if (!siteRoot.EndsWith("/", StringComparison.Ordinal)) siteRoot = siteRoot + '/';
            return siteRoot;
        }
    }
}
