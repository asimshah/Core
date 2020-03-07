using System.Net;
using System.Net.Mail;

namespace Fastnet.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class MailSender
    {
        internal string SmtpServer { get; set; }
        internal string Username { get; set; }
        internal string Password { get; set; }
        internal int Port { get; set; }
        internal bool EnableSsl { get; set; }
        internal SmtpClient SmtpClient { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public MailSender(MailOptions options)
        {
            SmtpServer = options.SmtpServer;
            Username = options.Username;
            Password = options.Password;
            Port = options.Port;
            EnableSsl = options.EnableSsl;
            SmtpClient = new SmtpClient(SmtpServer);
            if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Password))
            {
                SmtpClient.UseDefaultCredentials = true;
            }
            else
            {
                SmtpClient.UseDefaultCredentials = false;
                SmtpClient.Credentials = new NetworkCredential(Username, Password);
                SmtpClient.EnableSsl = EnableSsl;
                SmtpClient.Port = Port;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        public void Send(string from, string to, string subject, string body, bool isHtml = true)
        {
            var mm = new MailMessage();
            mm.From = new MailAddress(from);
            mm.To.Add(to);
            mm.Subject = subject;
            mm.Body = body;
            mm.IsBodyHtml = isHtml;
            SmtpClient.Send(mm);
        }
    }
}
