using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace Magnum.Mail
{
    public class FileMailServer :
        IMailServer
    {
        private readonly string _directoryToWriteTo;

        public FileMailServer(string directoryToWriteTo)
        {
            if(!Directory.Exists(directoryToWriteTo)) throw new FileNotFoundException(directoryToWriteTo + " not found");

            _directoryToWriteTo = directoryToWriteTo;
        }

        public void Send(MailMessage mailMessage)
        {
            WriteToFile(mailMessage);
        }

        public void Send(IList<MailMessage> mailMessages)
        {
            foreach (var message in mailMessages)
            {
                WriteToFile(message);
            }
        }

        public void Send(string from, string to, string subject, string message)
        {
            WriteToFile(new MailMessage(from, to , subject, message));
        }

        private void WriteToFile(MailMessage message)
        {
            var sb = new StringBuilder();

            sb.Append("To: ");

            foreach (var address in message.To)
            {
                sb.AppendFormat("{0},", address);
            }

            sb.Length = sb.Length - 1;
            sb.Append(Environment.NewLine);

            sb.AppendFormat("From: {0}{1}", message.From, Environment.NewLine);
            sb.AppendFormat("Subject: {0}{1}", message.Subject, Environment.NewLine);
            sb.AppendFormat("{1}{0}{1}", message.Body, Environment.NewLine);

            string result = sb.ToString();

            File.WriteAllLines(Path.Combine(_directoryToWriteTo, Guid.NewGuid().ToString() + ".txt"), new [] {result});
        }
    }
}