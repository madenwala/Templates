using AppFramework.Core.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage;

namespace AppFramework.Core
{
    public partial class PlatformCore
    {
        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public EmailProvider EmailProvider
        {
            get { return GetService<EmailProvider>(); }
            private set { SetService(value); }
        }
    }
}

namespace AppFramework.Core.Services
{
    public sealed class EmailProvider : ServiceBase
    {
        #region Constructors

        internal EmailProvider()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Send an e-mail.
        /// </summary>
        /// <param name="subject">Subject of the message</param>
        /// <param name="body">Body of the message</param>
        /// <param name="toRecipients">To recipients</param>
        /// <param name="ccRecipients">CC recipients</param>
        /// <param name="bccRecipients">BCC recipients</param>
        /// <param name="attachments">File attachments to the message.</param>
        /// <returns>Awaitable task is returned.</returns>
        public async Task SendEmailAsync(string subject, string body, string[] toRecipients, string[] ccRecipients, string[] bccRecipients, params IStorageFile[] attachments)
        {
            if (toRecipients == null || toRecipients.Length == 0)
                throw new ArgumentNullException(nameof(toRecipients));

            PlatformCore.Core.Analytics.Event("SendEmail");
            var msg = new EmailMessage();

            if (toRecipients != null)
                foreach (var address in toRecipients)
                    msg.To.Add(new EmailRecipient(address));

            if (ccRecipients != null)
                foreach (var address in ccRecipients)
                    msg.CC.Add(new EmailRecipient(address));

            if (bccRecipients != null)
                foreach (var address in bccRecipients)
                    msg.Bcc.Add(new EmailRecipient(address));

            msg.Subject = subject;
            msg.Body = body;

            if (attachments != null)
            {
                foreach (IStorageFile file in attachments)
                {
                    var stream = global::Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(file);
                    var ea = new EmailAttachment(file.Name, stream);
                    msg.Attachments.Add(ea);
                }
            }

            await EmailManager.ShowComposeNewEmailAsync(msg);
        }

        /// <summary>
        /// Send an e-mail.
        /// </summary>
        /// <param name="subject">Subject of the message</param>
        /// <param name="body">Body of the message</param>
        /// <param name="toRecipients">To recipients</param>
        /// <param name="attachments">File attachments to the message.</param>
        /// <returns>Awaitable task is returned.</returns>
        public Task SendEmailAsync(string subject, string body, string toRecipient, params IStorageFile[] attachments)
        {
            return this.SendEmailAsync(subject, body, new string[] { toRecipient }, null, null, attachments);
        }

        /// <summary>
        /// Send an e-mail.
        /// </summary>
        /// <param name="subject">Subject of the message</param>
        /// <param name="body">Body of the message</param>
        /// <param name="toRecipients">To recipients</param>
        /// <param name="ccRecipients">CC recipients</param>
        /// <param name="attachments">File attachments to the message.</param>
        /// <returns>Awaitable task is returned.</returns>
        public Task SendEmailAsync(string subject, string body, string[] toRecipients, string[] ccRecipients = null, params IStorageFile[] attachments)
        {
            return this.SendEmailAsync(subject, body, toRecipients, ccRecipients, null, attachments);
        }

        #endregion
    }
}
