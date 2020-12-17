using MailClient;
using Microsoft.Extensions.Configuration;
using System;

namespace TestMail
{
    class Program
    {

         private static IConfigurationRoot config;

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
       .AddJsonFile("testmail.json");
            config = builder.Build();

            ExternalMail mailClient = new ExternalMail();

            MailInformation newMail = new MailInformation();

           newMail.fromName = config["Mail:FromName"];
           newMail.fromEmail = config["Mail:FromEmail"];
           newMail.toName = config["Mail:ToName"];
           newMail.toEmail = config["Mail:ToEmail"];
           newMail.body = config["Mail:MmailBody"];
           newMail.subject = config["Mail:MailSubject"];
            newMail.attachment = ""; //add an attachment here for testing

                  mailClient.SendThisMail(newMail);

        }
    }
}
