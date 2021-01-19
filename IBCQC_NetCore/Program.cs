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

            //to call we use the exe plus three arguments port certname and password 

            if (args.Length != 3)
            {
                return;
            
            }
          
                CreateHostBuilder(args).Build().Run();
          
        }



        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                   .ConfigureHostConfiguration(webBuilder => { webBuilder.AddJsonFile($"RegisteredUsers.json", optional: true, reloadOnChange: true); })
                   .ConfigureWebHostDefaults(webBuilder =>
                   {
                       int port = 443;
                       if (args.Length > 0)
                       {
                           int.TryParse(args[0],out port);
                       
                       }

                       webBuilder.UseStartup<Startup>();
                       webBuilder.ConfigureKestrel(o =>
                       {
                           o.ConfigureHttpsDefaults(o =>
                               o.ClientCertificateMode = ClientCertificateMode.RequireCertificate
                           );

                           
                           o.ListenAnyIP(port, ListenOptions => { ListenOptions.UseHttps(args[1], args[2]); });
                           

                       });
                   });
        }

    }
}
