using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IBCQC_NetCore
{
    // Get a configuration class

    public class Program
    {

        // Moved to kestrel
        public static void Main(string[] args)
        {
            // Have option to remove certificate auth if we want, but need the serial number
            // so for testing that would need injecting

            bool certificateRequired = true;

            if (certificateRequired)
            {
                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                CreateHostBuilderNonSecure(args).Build().Run();
            }
        }


        public static IHostBuilder CreateHostBuilderNonSecure(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
             .ConfigureServices((context, services) =>
             {
                 services.Configure<KestrelServerOptions>(
                     context.Configuration.GetSection("Config:Kestrel"));
             })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                   
                    logging.AddConsole(options => options.IncludeScopes = true);

                    // Debug Logging Provider:
                    // The Debug provider writes log output by using the System.Diagnostics.Debug class.
                    // Calls to System.Diagnostics.Debug.WriteLine write to the Debug provider.
                    // On Linux, the Debug provider log location is distribution-dependent
                    // and may be one of the following:
                    //   /var/log/message
                    //   /var/log/syslog
                    //logging.AddDebug();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.ConfigureKestrel(o =>
                    {
                        o.ConfigureHttpsDefaults(o =>
                            //o.ClientCertificateMode =  ClientCertificateMode.RequireCertificate
                            o.ClientCertificateMode =  ClientCertificateMode.NoCertificate
                       );
                    });
                });
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
                               o.ClientCertificateMode = ClientCertificateMode.RequireCertificate
                           );
                       });
                   });
        }

    }
}
