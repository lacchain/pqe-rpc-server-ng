using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IBCQC_NetCore
{

    //get a configuration class
  


    public class Program
    {

        //moved to kestrel
        public static void Main(string[] args)
        {

            //have option to remove  certificate auth if we want but need the serail number so for testing that woudl need injecting 

            bool certificateRequired = true;

            if (certificateRequired)
            { CreateHostBuilder(args).Build().Run(); }

            else
            {
                CreateHostBuilderNonSecure(args).Build().Run();

            }

           
        }


        public static IHostBuilder CreateHostBuilderNonSecure(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
              
          
                
                .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
                  
                  
                   webBuilder.ConfigureKestrel(o =>
                   {         
                      
                       o.ConfigureHttpsDefaults(o =>
                                o.ClientCertificateMode =  ClientCertificateMode.RequireCertificate);
                   });
               }
               
               
               
               );
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                 .ConfigureHostConfiguration(webBuilder => { webBuilder.AddJsonFile($"RegisteredUsers.json", optional: true, reloadOnChange: true); })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                  webBuilder.ConfigureKestrel(o =>
                    {
                        o.ConfigureHttpsDefaults(o =>
                    o.ClientCertificateMode =
                        ClientCertificateMode.RequireCertificate);
                    });
                });
        }



    }
}