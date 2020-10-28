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

        
          
                CreateHostBuilder(args).Build().Run();
          
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
