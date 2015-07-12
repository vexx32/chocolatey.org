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

        private static void AddOwnersToMailMessage(
            PackageRegistration packageRegistration, MailMessage mailMessage, bool requireEmail = false)
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
https://github.com/chocolatey/choco/wiki/CreatePackages

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
            using (
                var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = new MailAddress(settings.GalleryOwnerEmail, settings.GalleryOwnerName);

                mailMessage.To.Add(new MailAddress(oldEmailAddress, user.Username));
                SendMessage(mailMessage);
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

        public void SendPackageModerationEmail(Package package, string comments)
        {
            string subject = "[{0}] Moderation for '{1}' v{2}";
            var packageUrl = string.Format(
                "{0}packages/{1}/{2}",
                EnsureTrailingSlash(Configuration.ReadAppSettings("SiteRoot")),
                package.PackageRegistration.Id,
                package.Version);
            string body = @"'{0}' is {3}{6}.
{4}

Package Url: {1} 
Maintainer(s): {2}
{5}
";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                package.PackageRegistration.Id,
                packageUrl,
                string.Join(", ", package.PackageRegistration.Owners.Select(x => x.Username)),
                package.Status.GetDescriptionOrValue(),
                GetModerationMessage(package, comments),
                GetInformationForMaintainers(package, comments),
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
                mailMessage.From = new MailAddress(
                    Configuration.ReadAppSettings("ModeratorEmail"), settings.GalleryOwnerName);

                AddOwnersToMailMessage(package.PackageRegistration, mailMessage, requireEmail: true);
                //mailMessage.To.Add(settings.GalleryOwnerEmail);
                if (mailMessage.To.Any()) SendMessage(mailMessage);
            }
        }

        private string GetDisqusInformationMessage()
        {
            return @"
## DO NOT REPLY TO THIS MESSAGE!

### Information for Maintainers

 * Disqus comments can be moderated.  As a result, you may not see the above comment on the package page until the comment is moderated.
 * If the comment seems legitimate, i.e. not a spam comment, you can take action straight away.
 * You will not be able to reply to the Disqus Comment until it is moderated.
 * You are encouraged to reply directly to the Disqus Comment when required action is taken.
";
        }

        private string GetInformationForMaintainers(Package package, string comments)
        {
            if (package.Status == PackageStatusType.Submitted && string.IsNullOrWhiteSpace(comments)) return @"
**NOTICE:** Currently we have a very large backlog (popularity is a double-edged sword) and are addressing it, but as a result moderation may take 
upwards of two or more weeks until we resolve issues.

For urgent issues (including packages that are security releases), please reach out to us immediately on Gitter at https://gitter.im/chocolatey/choco 

Things we are doing to help resolve the large backlog of moderation:

 * Adding in a service to automatically verify the install and uninstall of packages.
 * Adding some auto-moderation for things a computer can check in a package.
 * Adding more moderators

### Information for Maintainers

 * If you have fixes, repush your package with the **same version**. This is allowed until approved.
 * Reply to this message with questions/comments. 

#### Other Pertinent Information

 * Moderators typically review a package within about a week or less. 
 * If you have not heard anything within a week, please reply to this message and ask for status.
 * If the package is an urgent release (resolves security issues or CVEs), reach out to use immediately on Gitter at https://gitter.im/chocolatey/choco
 * Packages must conform to our guidelines https://github.com/chocolatey/choco/wiki/CreatePackages
 * Packages typically get rejected for not conforming to our naming guidelines - https://github.com/chocolatey/choco/wiki/CreatePackages#naming-your-package
";
            else if (package.Status == PackageStatusType.Submitted) return @"
#### Maintainer Notes

 * If we've asked you to make changes, repush your updated package with the **_same_ version**.";

            return string.Empty;
        }

        private string GetModerationMessage(Package package, string comments)
        {
            var message = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(comments))
            {
                message.AppendFormat(
                    "{0} left the following comment(s):{1}", package.ReviewedBy.Username, Environment.NewLine);
                message.Append(Environment.NewLine + comments);
            } else if (package.Status == PackageStatusType.Rejected)
            {
                message.AppendFormat(
                    "The moderator left the following comment(s):{1}", package.ReviewedBy.Username, Environment.NewLine);
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
                        "{3}{3}The package was {0} by moderator {1} on {2}.",
                        package.Status.GetDescriptionOrValue().ToLower(),
                        package.ReviewedBy.Username,
                        package.ReviewedDate.GetValueOrDefault().ToShortDateString(),
                        Environment.NewLine);
                    break;
            }

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
