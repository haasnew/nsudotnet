using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Rss2Email
{
    class Program
    {
        private const string SourceEmail = "test@test.test";
        private const string KnownIdsFileName = "knownIds.dat";
        static void Main()
        {
            Console.Write("Укажите URL:");
            var url = "http://bash.im/rss";  
            Console.WriteLine(url);
            Console.Write("email получателя:");

            var targetEmail = "khasanova_asel@outlook.com";
            Console.WriteLine(targetEmail);
            var xmlReader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(xmlReader);
            xmlReader.Close();

            var _sender = "khasanova_asel@outlook.com";
            var _password = "aigerim123";

            using (var client = new SmtpClient("smtp-mail.outlook.com")
            {
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(_sender, _password)
            })
            {
                List<string> knownIds;
                try
                {
                    knownIds = File.ReadAllLines(KnownIdsFileName).ToList();
                }
                catch
                {
                    knownIds = new List<string>();
                }

                foreach (var post in feed.Items.Where(x => !knownIds.Contains(x.Id)).Take(3))
                {
                    Console.WriteLine("отправка сообщений {0}", post.Id);
                    var message = new MailMessage(SourceEmail, targetEmail, post.Title.Text, post.Summary.Text) { IsBodyHtml = true };
                    client.Send(message);
                    knownIds.Add(post.Id);
                }

                File.WriteAllLines(KnownIdsFileName, knownIds);
            }
        }
    }
}
