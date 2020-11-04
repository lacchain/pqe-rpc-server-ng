using System;
using System.Linq;
using System.Security.Claims;
using IBCQC_NetCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResetClientController : ControllerBase
    {

        private static CallerInfo callerInfo;

        private readonly ILogger<ResetClientController> _logger;
        public ResetClientController(ILogger<ResetClientController> logger)
        {
            _logger = logger;
        }





        // GET: api/<ResetClientController>
        [HttpGet]
  
        [Authorize]
         public IActionResult Get(string removeCertSerial)
        {

            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] remove client called");


            // Go get from auth claims
            ClaimsPrincipal currentUser = this.User;

            // As this is the authenticated cert we get a number of claims from the authentication handler
            // issuer thumbprint x500distinguisehedname name serial and dns   
            string certSerial = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value;
            string friendlyName = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            string thumbprint = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Thumbprint)?.Value;

            if (certSerial == null)
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ReqKeyPair No Certificate Serial Number");
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
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ReqKeyPair No Certificate Friendly Namer");
                return StatusCode(401, "No Friendly Name associated with this certificate");
            }

            //check certificate is one of our registered certificates


            try
            {
                callerInfo = RegisterNodes.GetClientNode(certSerial, Startup.StaticConfig["Config:clientFileStore"]);

                // OK -is this a known serial certificate
                if (string.IsNullOrEmpty(callerInfo.callerID))
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ResetClient Unknown Certificate ");
                    return StatusCode(401, "Unknown Certificate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ResetClient Cannot Identify User");
                return StatusCode(500, "Cannot identify caller. Exception: " + ex.Message);
            }





            //ok process the request

            bool clientRemoved = RegisterNodes.RemoveClientNode(removeCertSerial, Startup.StaticConfig["Config:clientFileStore"]);

            if (clientRemoved)
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] client with certificate was removed");

                return StatusCode(200, "Client was removed");

            }

            else
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] client was not removed");

                return StatusCode(401, "Client was not removed");
            }

        }

    }
}
