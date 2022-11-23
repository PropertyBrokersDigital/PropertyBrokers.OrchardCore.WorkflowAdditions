using OrchardCore.Email;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.EmailFile
{
    /// <summary>
    /// Represents a contract for SMTP service.
    /// </summary>
    public interface ISmtpService
    {
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A <see cref="SmtpResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
        Task<SmtpResult> SendAsync(MailMessage message);
    }
}
