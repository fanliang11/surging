using System.Collections.Generic;
using System.Net.Mail;

namespace Magnum.Mail
{
    public class NullMailServer :
        IMailServer
    {
        public void Send(MailMessage mailMessage)
        {
            //do nothing
        }

        public void Send(IList<MailMessage> mailMessages)
        {
            //do nothing
        }

        public void Send(string from, string to, string subject, string message)
        {
            //do nothing
        }
    }
}