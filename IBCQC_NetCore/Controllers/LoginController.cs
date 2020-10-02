﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
//using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Certificate;

using IBCQC_NetCore.ViewModel;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using System.Security.Claims;

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
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

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
                // Go get from auth claims
                ClaimsPrincipal currentUser = this.User;

                // As this is the authenticated cert we get a number of claims from the authentication handler
                // issuer thumbprint x500distinguisehedname name serial and dns   
                string certSerial = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value;
                string friendlyName  = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                string thumbprint = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Thumbprint)?.Value;

                if (certSerial == null)
                {
                    return StatusCode(401, "No Serial Number retrieved from Certificate");
                }


                // Certificate Serial Number
                if (certSerial.Length < 18)
                {
                    certSerial = certSerial.PadLeft(18, '0');
                }

                //Friendly Certificate Name 
                string certFriendlyName = friendlyName;   
                if (certFriendlyName == null)
                {
                    return StatusCode(401, "No Friendly Name associated with this certificate");
                }

                //check certificate is one of our registered certificates

                RegisterNodes chkNode = new RegisterNodes();
                try
                {
                  CallerInfo  callerInfo = chkNode.GetClientNode(certSerial, Startup.StaticConfig["Config:clientFileStore"]);

                    // OK -is this a known serial certificate
                    if (string.IsNullOrEmpty(callerInfo.callerID))
                    {
                        return StatusCode(401, "Unknown Certificate");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Cannot identify caller. Exception: " + ex.Message);
                }

                JwtTokenHandler getToken = new JwtTokenHandler();

                var tokenExpiry = Startup.StaticConfig["Config:jwt_lifetime"];
                var tokenSecret = Startup.StaticConfig["Config:jwt_secret"];
                var tokenIssuer = Startup.StaticConfig["Config:jwt_issuer"];
                var tokenAudience = Startup.StaticConfig["Config:jwt_audience"];

                var jwtTok =  getToken.GenerateToken(certSerial,tokenExpiry,tokenSecret,tokenIssuer,tokenAudience);

                // Send the usual token format
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
                return Unauthorized( "*401* Login failed with exception: " + ex.Message);
            }
        }


        [HttpPost]
        public IActionResult Login( string posted)
        {
            try
            {
                //cert Serial Number
                var cert = Request.HttpContext.Connection.ClientCertificate;
                // Get the public key
                byte[] userPublicKey = cert.GetPublicKey();
                string certSerial = cert.SerialNumber;
                if (certSerial.Length < 18)
                {
                    certSerial = certSerial.PadLeft(18, '0');
                }
                if (certSerial == null)
                {
                    return Unauthorized("No Serial Number retrieved from Certificate");
                }
                //Friendly Certificate Name
                string certFriendlyName = cert.FriendlyName;
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
                _logger.LogInformation("ERROR: Login failed with exception: " + ex.Message);
                return Unauthorized("*401* Login failed. ");
            }
        }


    }
}
