using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


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
                      
                           //use to set to port 443 and apply a certificate
                               o.ListenAnyIP(32770, ListenOptions => { ListenOptions.UseHttps("testing.ironbridgeapi.com.pfx", "$London123"); });                       });
                   });
        }

    }
}
