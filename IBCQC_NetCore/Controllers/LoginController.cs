using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using IBCQC_NetCore.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Certificate;
using System.Security.Claims;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using System.Text.Json;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    /// <summary>
    /// new code
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// This is the basic username password login method
    /// it provides a user token
    /// TODO override if a user cert is present
    /// </summary>
    /// 


    //  api/<LoginController>

    [AllowAnonymous]

    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {

  
        /// <summary>
        /// Provides a token for using this API
        /// </summary>
        /// Login using client certificate
        /// <returns></returns>
        /// 

       
        [HttpGet]
   

        public IActionResult Login()
        {
            try
            {
             

                //cert Serial Number


                var cert = Request.HttpContext.Connection.ClientCertificate;

                // Get the public key
                byte[] userPublicKey = cert.GetPublicKey();

              string  certSerial = cert.SerialNumber;


                if (certSerial.Length < 18)
                {
                    certSerial = certSerial.PadLeft(18, '0');
                }



              //  string certSerial = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value;
                if (certSerial == null)
                {
                    return Unauthorized("No Serial Number retrieved from Certificate");
                }

                //Friendly Certificate Name 
                string certFriendlyName = cert.FriendlyName;   //currentUser.Claims.FirstOrDefault( c => c.Type == ClaimTypes.Name)?.Value;
                if (certFriendlyName == null)
                {
                    return Unauthorized( "No Friendly Name Associated with this certificate");
                }


                JwtTokenHandler getToken = new JwtTokenHandler();


                var tokenExpiry = Startup.StaticConfig["Config:jwt_lifetime"];
                var tokenSecret = Startup.StaticConfig["Config:jwt_secret"];
                var tokenIssuer = Startup.StaticConfig["Config:jwt_issuer"];
                var tokenAudience = Startup.StaticConfig["Config:jwt_audience"];

                var jwtTok =  getToken.GenerateToken(certSerial,tokenExpiry,tokenSecret,tokenIssuer,tokenAudience);





                //send the usual token format

                JwtTokenResult issueToken = new JwtTokenResult()
                {
                    Token = jwtTok,
                    NotBefore = DateTime.UtcNow,
                    NotAfter = DateTime.UtcNow.AddHours( 7)

                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };




                var userJwtTok = JsonSerializer.Serialize(issueToken, options);

                return Ok(userJwtTok);
            }
            catch (Exception ex)
            {
                // APILogging.Log("Login", "ERROR: Login failed with exception: " + ex.Message);
                return Unauthorized( "*401* Login failed. ");
            }
        }





        [HttpPost]

        public IActionResult Login([FromBody] string posted)
        {
            try
            {
                // Certificate request

                //As the requirement for client cert is baked into server and also through DI 
                //see program.cs for host builder and  Startup.cs for the certificate fail and success 
                //actions Success will populate the claimsprincipal in the controller base

                //todo extend the activation to turn off certs if required for testing

                //this will retun a forbidden if no vaid cert is found this can b extended to only allow registered cluent certificate sif required

                ClaimsPrincipal currentUser = this.User;
                //get the claims from the certificate context

                //we have dual claims at this moment defind in startuop.cs

                //cert Serial Number

                string certSerial = currentUser.Claims.FirstOrDefault(
             c => c.Type == ClaimTypes.SerialNumber)?.Value;
                if (certSerial == null)
                {
                    return Unauthorized("No Serial Number retrieved from Certificate");
                }

                //Friendly Certificate Name 
                string certFriendlyName = currentUser.Claims.FirstOrDefault(
             c => c.Type == ClaimTypes.Name)?.Value;
                if (certFriendlyName == null)
                {
                    return Unauthorized("No Friendly Name Associated with this certificate");
                }


                JwtTokenHandler getToken = new JwtTokenHandler();


                var tokenExpiry = Startup.StaticConfig["Config:jwt_lifetime"];
                var tokenSecret = Startup.StaticConfig["Config:jwt_secret"];
                var tokenIssuer = Startup.StaticConfig["Config:jwt_issuer"];
                var tokenAudience = Startup.StaticConfig["Config:jwt_audience"];

                var jwtTok = getToken.GenerateToken(certSerial, tokenExpiry, tokenSecret, tokenIssuer, tokenAudience);





                //send the usual token format

                JwtTokenResult issueToken = new JwtTokenResult()
                {
                    Token = jwtTok,
                    NotBefore = DateTime.UtcNow,
                    NotAfter = DateTime.UtcNow.AddHours(7)

                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };




                var userJwtTok = JsonSerializer.Serialize(issueToken, options);

                return Ok(userJwtTok);
            }
            catch (Exception ex)
            {
                // APILogging.Log("Login", "ERROR: Login failed with exception: " + ex.Message);
                return Unauthorized("*401* Login failed. ");
            }
        }

    }
}


        