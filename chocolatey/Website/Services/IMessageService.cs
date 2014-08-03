using System.Net.Mail;

namespace NuGetGallery
{
    public interface IMessageService
    {
        void SendContactOwnersMessage(MailAddress fromAddress, PackageRegistration packageRegistration, string message, string emailSettingsUrl,string packageUrl, bool copySender);
        void ReportAbuse(MailAddress fromAddress, Package package, string message, string packageUrl, bool copySender);
        void ContactSiteAdmins(MailAddress fromAddress, Package package, string message, string packageUrl, bool copySender);
        void SendNewAccountEmail(MailAddress toAddress, string confirmationUrl);
        void SendEmailChangeConfirmationNotice(MailAddress newEmailAddress, string confirmationUrl);
        void SendPasswordResetInstructions(User user, string resetPasswordUrl);
        void SendEmailChangeNoticeToPreviousEmailAddress(User user, string oldEmailAddress);
        void SendPackageOwnerRequest(User fromUser, User toUser, PackageRegistration package, string confirmationUrl);
    }
}