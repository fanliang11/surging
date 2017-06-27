using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace Magnum.Mail
{
    public class SmtpMailServer :
        IMailServer
    {
        private readonly SmtpClient _client;

        public SmtpMailServer(string address) : this (address, "","")
        {
            
        }
        public SmtpMailServer(string address, string username, string password)
        {
            _client = new SmtpClient(address);

            if(!string.IsNullOrEmpty(username))
            {
                _client.Credentials = new NetworkCredential(username, password);
            }
        }

        public void Send(MailMessage mailMessage)
        {
            _client.Send(mailMessage);
        }

        public void Send(IList<MailMessage> mailMessages)
        {
            foreach (var message in mailMessages)
            {
                Send(message);
            }
        }

        public void Send(string from, string to, string subject, string message)
        {
            _client.Send(from, to, subject,message);
        }
    }
}