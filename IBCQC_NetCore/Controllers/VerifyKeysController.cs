
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.Rng;
using static IBCQC_NetCore.Models.ApiEnums;
using System.Security.Claims;



namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifyKeysController : ControllerBase
    {

        private string certSerial;

        private static CallerInfo callerInfo;
            private readonly ILogger<VerifyKeysController> _logger;

        public VerifyKeysController(ILogger<VerifyKeysController> logger)
        {
            _logger = logger;
        }


       
        public IActionResult Post([FromBody] KeyValidate keyValidate)
        {

            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] VerifyKeys called  ");


            try
            {
               

                // Is this the request or response
                string requestType = keyValidate.requestType;

                // Cert req if passed validation then we have a client cert
                bool ignoreClientCertificateErrors = Convert.ToBoolean(Startup.StaticConfig["Config:IgnoreClientCertificateErrors"]);
                if (ignoreClientCertificateErrors)
                {
                    return StatusCode(401,"WARNING: Not supported while Client Certificate checks are disabled");
                }

                // Go get from auth claims
                ClaimsPrincipal currentUser = this.User;

          
                certSerial = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value;
                string friendlyName = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
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

               
                try
                {
                   callerInfo = RegisterNodes.GetClientNode(certSerial, Startup.StaticConfig["Config:clientFileStore"]);

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

               

                bool isValidCaller = CallerValidateFunction.callerValidate(callerInfo, CallerStatus.requireKemValid);
                // They need a valid KEM key, not a shared secret
                if (!isValidCaller)
                {
                    if (CallerValidateFunction.kemKeyPairNeedsChanging)
                    {
                        return StatusCode(498, "KemKeyPair Not Valid)");// Content((System.Net.HttpStatusCode)498 /*TokenExpiredOrInvalid*/, "KEM KeyPair not valid");
                    }
                    else
                    {
                        return StatusCode(401,"Client unknown or invalid");
                    }
                }



            }
            catch (Exception ex)
            {
                _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                return Unauthorized("Unable to locate security parameters for client");
            }

            try
            {


                switch (keyValidate.typeOfKey.ToLower())
                {
                    // For a private key validation we need a valid public key pair.
                    // We encode a random string. They will decode and re-encode with the private key and resubmit
                    case "kem":
                        {

                            //use KemKeyValidationFunction   
                            return StatusCode(400, "In development");
                        }
                    case "aes":
                        {
                            //use AESKeyValidationFunction
                            return StatusCode(400, "In development");

                        }
                    default:
                        return StatusCode(400, "In development");
                }
            }

            catch (Exception ex)
            {
                _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                return StatusCode(500, "VerifyKeys failed with exception: " + ex.Message);
            }
        }


    }
}
