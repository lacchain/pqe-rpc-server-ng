using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IBCQC_NetCore.Models;
using static IBCQC_NetCore.Models.ApiEnums;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Rng;
using IBCQC_NetCore.Encryption;
using Microsoft.AspNetCore.Authorization;
using IBCQC_NetCore.OqsdotNet;
using Microsoft.Extensions.Logging;

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReqKeyPairController : ControllerBase
    {

        private static CallerInfo callerInfo;

        private readonly ILogger<ReqKeyPairController> _logger;
        public ReqKeyPairController(ILogger<ReqKeyPairController> logger)
        {
            _logger = logger;
        }



        // GET: api/<ReqKeyPairController>
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {

            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ReqKeyPairCalled called");


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
                 callerInfo =RegisterNodes.GetClientNode(certSerial, Startup.StaticConfig["Config:clientFileStore"]);

                // OK -is this a known serial certificate
                if (string.IsNullOrEmpty(callerInfo.callerID))
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ReqKeyPair Unknown Certificate ");
                    return StatusCode(401, "Unknown Certificate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ReqKeyPair Cannot Identify User");
                return StatusCode(500, "Cannot identify caller. Exception: " + ex.Message);
            }

          

            // First check if this is intialise or not.
            // If initialise then they would fail the validation checks
            if (!Convert.ToBoolean(callerInfo.isInitialise))
            {
                bool isValidCaller = CallerValidateFunction.callerValidate(callerInfo, CallerStatus.requireSharedValid);
                if (!isValidCaller)
                {
                    if (CallerValidateFunction.sharedSecretNeedsChanging)
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ReqKeyPair Shared Secret Expired");

                        return StatusCode(498, "SharedSecret has expired");
                    }
                    else
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] ReqKeyPair Client Unknown");

                        return StatusCode(401, "Client unknown or invalid");
                    }
                }
            }



            // Get the keypair
            try
            {

                // As this is standalone we use bouncy castle

                Prng getRandom = new Prng();
                //so get 256 Byte key for AES

                // Set number of iterations for the RFC2898 derivation function
                // to a reasonably large number, and let's choose a prime number for fun.
              
                int iterations = Convert.ToInt16(Startup.StaticConfig["Config:DerivationIterations"]);
              
                int saltSize = Convert.ToInt16(Startup.StaticConfig["Config:SaltSize"]);

                byte[] saltBytes = getRandom.GetBytes(saltSize);


              
                byte[] encryptedBytes1;   // Byte array to hold encrypted data
                string sendBytes;         // String to hold send data

                // OK - implement the AES encryption
                AESEncrypt encryptAES = new AESEncrypt();

                //need the correct name for the algorithm 

              

                var algoRequested = Enum.GetName(typeof(SupportedAlgorithmsEnum), Convert.ToInt16(callerInfo.kemAlgorithm)); 

                //because enum has no hypen 
                algoRequested = algoRequested.Replace("_", "-");

                using (KEM client = new KEM(algoRequested))
                {
                   
                    // Generate the client's key pair
                    byte[] public_key;
                    byte[] secret_key;
                    client.keypair(out public_key, out secret_key);

                        // Now use AES




                        encryptedBytes1 = encryptAES.Encrypt(secret_key,
                                                                     Convert.FromBase64String(callerInfo.sharedSecretForSession),
                                                                     saltBytes,
                                                                     iterations);

                   // Send as base64

                    sendBytes = Convert.ToBase64String(encryptedBytes1);

                    //update the public key

                    RegisterNodes.UpdKemPublicKey(sendBytes, Startup.StaticConfig["Config:clientFileStore"], certSerial);

                    bool isdebug = false;
                    try
                    {
                        isdebug = Convert.ToBoolean(Startup.StaticConfig["Config:OutputDebugKeys"]);

                    }
                    catch 
                    { 
                    //nothing to do if debug not set
                    }

                    if (isdebug)
                    {
                        string dbgPrivateKey = Convert.ToBase64String(secret_key);

                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from Request Key Pair Call ");

                        return StatusCode(200, "::The private key encrypted is ::" + sendBytes + "::The private key in Base64 is ::" + dbgPrivateKey);

                    }
                    else
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from Request Key Pair Call ");
                        return StatusCode(200, sendBytes);
                    }
                }



                      
                        
                        
                   

                  
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Error from Request Key Pair Call ::" + ex.Message);

                return StatusCode(400, "Unsupported KEM Algorithm" + ex.Message);
            }


  







        }






  
    }
}
