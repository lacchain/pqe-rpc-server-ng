using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace IBCQC_NetCore
{
    public class Startup
    {
        //Load a static private configuration for use elsewhere

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticConfig = configuration;
        }

        public IConfiguration Configuration { get; }
        public static IConfiguration StaticConfig { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

           
      

            // Add certificate auth if we want it

            bool ignoreCertificate = Convert.ToBoolean(Configuration["Config:ignoreClientCertificateErrors"]);
            if (!ignoreCertificate)
            {
                services.AddAuthentication(
                    CertificateAuthenticationDefaults.AuthenticationScheme)
                    .AddCertificate(options =>
                    {
                        options.RevocationMode = X509RevocationMode.NoCheck;
                        options.AllowedCertificateTypes = CertificateTypes.All;
                        options.ValidateCertificateUse = false;
                     
                        services.AddAuthentication(
                            CertificateAuthenticationDefaults.AuthenticationScheme)
                            .AddCertificate(options =>
                            {
                                options.Events = new CertificateAuthenticationEvents
                                {
                                    //  check the certificate options and return
                                    OnCertificateValidated = context =>
                                    {
                                        // We do not ned to add claims the cert auth does that as the defaulty idetity
                                       
                                        context.Success();

                                        return Task.CompletedTask;
                                    },
                                    OnAuthenticationFailed = context =>
                                    {
                                        context.Success();
                                       // context.Fail("invalid cert");
                                        return Task.CompletedTask;
                                    }

                                };
                            });
                    });
            } 



            
            services.AddControllers();
        }
        // Add the controller service

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
