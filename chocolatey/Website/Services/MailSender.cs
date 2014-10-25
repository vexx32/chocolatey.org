namespace NuGetGallery
{
    using System;
    using System.IO;
    using System.Net.Mail;
    using System.Net.Mime;
    using AnglicanGeek.MarkdownMailer;
    using MarkdownSharp;


    /// <summary>
    /// This is an override of MarkdownMailer.MailSender
    /// </summary>
    /// <remarks>https://github.com/half-ogre/MarkdownMailer/blob/master/LICENSE.txt</remarks>
    public class MailSender : IMailSender
    {

        readonly SmtpClient _smtpClient;

        public MailSender() : this(new SmtpClient(), null)
        {
        }

        public MailSender(MailSenderConfiguration configuration) : this(new SmtpClient(), configuration)
        {
        }

        public MailSender(SmtpClient smtpClient): this(smtpClient, null)
        {
        }

        internal MailSender(SmtpClient smtpClient,MailSenderConfiguration configuration)
        {
            if (smtpClient == null) throw new ArgumentNullException("smtpClient");

            if (configuration != null) ConfigureSmtpClient(smtpClient, configuration);

            this._smtpClient = smtpClient;
        }

        static internal void ConfigureSmtpClient(SmtpClient smtpClient,MailSenderConfiguration configuration)
        {
            if (configuration.Host != null) smtpClient.Host = configuration.Host;
            if (configuration.Port.HasValue) smtpClient.Port = configuration.Port.Value;
            if (configuration.EnableSsl.HasValue) smtpClient.EnableSsl = configuration.EnableSsl.Value;
            if (configuration.DeliveryMethod.HasValue) smtpClient.DeliveryMethod = configuration.DeliveryMethod.Value;
            if (configuration.UseDefaultCredentials.HasValue) smtpClient.UseDefaultCredentials = configuration.UseDefaultCredentials.Value;
            if (configuration.Credentials != null) smtpClient.Credentials = configuration.Credentials;
            if (configuration.PickupDirectoryLocation != null) smtpClient.PickupDirectoryLocation = configuration.PickupDirectoryLocation;
        }

        public void Send(string fromAddress, string toAddress, string subject, string markdownBody)
        {
            Send(new MailAddress(fromAddress),new MailAddress(toAddress),subject,markdownBody);    
        }

        public void Send(MailAddress fromAddress, MailAddress toAddress, string subject, string markdownBody)
        {
            var mailMessage = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = markdownBody
                };

            Send(mailMessage);
        }

        public void Send(MailMessage mailMessage)
        {
            if (_smtpClient.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory && !Directory.Exists(_smtpClient.PickupDirectoryLocation))
            {
                Directory.CreateDirectory(_smtpClient.PickupDirectoryLocation);
            }
            
            string markdownBody = mailMessage.Body;
           

            AlternateView textView = AlternateView.CreateAlternateViewFromString(
                markdownBody,
                null,
                MediaTypeNames.Text.Plain);
            
            mailMessage.AlternateViews.Add(textView);

            //this is what is different. Tired of receiving plaintext messages with the markdown crap in them.
            var markdownGenerator = new Markdown(new MarkdownOptions
                {
                    AutoHyperlink = true,
                    AutoNewLines = true,
                    EncodeProblemUrlCharacters = true,
                    LinkEmails = true,
                });

            string htmlBody = markdownGenerator.Transform(markdownBody);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                htmlBody,
                null,
                MediaTypeNames.Text.Html);
            mailMessage.AlternateViews.Add(htmlView);

            _smtpClient.Send(mailMessage);
        }
    }
}