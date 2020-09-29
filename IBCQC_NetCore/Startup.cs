using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IBCQC_NetCore.Models;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            services.AddControllers();
         // add certificate auth if we want it 

            bool certificateRequired = Convert.ToBoolean(Configuration["Config:ignoreClientCertificateErrors"]);

            if (certificateRequired)
            {
                services.AddAuthentication(
                    CertificateAuthenticationDefaults.AuthenticationScheme)
                    .AddCertificate(options =>
                    {

                        services.AddAuthentication(
                           CertificateAuthenticationDefaults.AuthenticationScheme)
                           .AddCertificate(options =>
                           {
                                       options.Events = new CertificateAuthenticationEvents
                                       {

                                       //  check the certificate options and return
                                               OnCertificateValidated = context =>
                                                                        {

                                                                            var claims = new[]
                                                                            {
                                                                                                    new Claim(
                                                                                                        ClaimTypes.SerialNumber,
                                                                                                        context.ClientCertificate.SerialNumber,
                                                                                                        ClaimValueTypes.String,
                                                                                                        context.Options.ClaimsIssuer),
                                                                                                    new Claim(
                                                                                                        ClaimTypes.Name,
                                                                                                        context.ClientCertificate.FriendlyName,
                                                                                                        ClaimValueTypes.String,
                                                                                                        context.Options.ClaimsIssuer)
                                                                                      };

                                                                            context.Principal = new ClaimsPrincipal(
                                                                                new ClaimsIdentity(claims, context.Scheme.Name));
                                                                            context.Success();


                                                                            return Task.CompletedTask;
                                                                        },
                                               OnAuthenticationFailed = context =>
                                                                       {
                                                                           context.Fail("invalid cert");
                                                                           return Task.CompletedTask;
                                                                       }

                                       };
                          });
                    });
            }
        }   
            //      //add the controller service



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
