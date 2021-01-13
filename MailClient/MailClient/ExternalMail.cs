using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;

namespace MailClient
{
    public class ExternalMail
    {

       private static  IConfigurationRoot  config;


        //constructor get config
      public ExternalMail()
        {
            var builder = new ConfigurationBuilder()
        .AddJsonFile("mailconfig.json");
            config = builder.Build();

        }



        public bool SendThisMail(MailInformation thisMail)
               
        {

         

            //ok get the config then get the mail class we expect
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(thisMail.fromName, thisMail.fromEmail));
            mailMessage.To.Add(new MailboxAddress(thisMail.toName,thisMail.toEmail));
            mailMessage.Subject = thisMail.subject;

            var builder = new BodyBuilder();

            // Set the plain-text version of the message text
            builder.TextBody = thisMail.body;

            //Add the attachment if it existss
            if (string.IsNullOrEmpty(thisMail.attachment))
            {
               
            
            }

            else
            { builder.Attachments.Add(thisMail.attachment); }

           

            // Now we just need to set the message body and we're done
            mailMessage.Body = builder.ToMessageBody();




           

            //must use mailkit version of smtp
            using (var smtpClient = new SmtpClient())
            {

                // SecureSocketOptions secOptions =  SecureSocketOptions.StartTls

                //465 for ssl connections

                //ok make surew config values are present 
                try
                {
                    string mailServer = config["Smtp:Host"]; //port username password

                    int port = Convert.ToInt16(config["Smtp:Port"]);

                    string userName = config["Smtp:Username"];

                    string passWord = config["Smtp:Password"];

                    bool ssl = Convert.ToBoolean(config["Smtp:Ssl"]);

                    if (string.IsNullOrEmpty(mailServer))
                    {
                        return false;

                    }
                 
                    smtpClient.Connect(mailServer, port, ssl);
                    if (!smtpClient.IsConnected)
                    {
                        return false;

                    }


                    if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
                    {
                        smtpClient.Authenticate(userName, passWord);
                    }

                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);

                    return true;
            
                }

                catch 
                {

                    return false;
                }


            
            }


     




     }


    }
}
