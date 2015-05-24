using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Xml;

namespace Rss2Email
{
    class Program
    {
        private const string SourceEmail = "test@test.test";
        static void Main()
        {
            Console.Write("Укажите URL:");
            var url = "http://www.5-tv.ru/news/rss/";
            Console.WriteLine(url);
            Console.Write("email получателя:");
           
            var targetEmail = "khasanova_asel@outlook.com";
            Console.WriteLine(targetEmail);
          

            var reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);
            reader.Close();

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
                var knownIds = new List<string>();

                while (true)
                {
                    foreach (var post in feed.Items.Where(x => !knownIds.Contains(x.Id)).Take(3))
                    {
                        Console.WriteLine("отправка сообщений {0}", post.Id);
                        var message = new MailMessage(SourceEmail, targetEmail, post.Title.Text, post.Summary.Text) { IsBodyHtml = true };
                        client.Send(message);
                        knownIds.Add(post.Id);
                    }
                    Console.WriteLine("going to sleep......");
                    Thread.Sleep(100);
                }
            }
        }
    }
}