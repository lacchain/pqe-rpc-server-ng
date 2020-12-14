using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IBCQC_NetCore.Encryption;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.Rng;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static IBCQC_NetCore.Models.ApiEnums;

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetChatSessionKeyController : ControllerBase
    {
        private static CallerInfo callerInfo;
        private static CallerInfo participatingPartyInfo;
        private static string certSerial;
        private readonly ILogger<GetChatSessionKeyController> _logger;

        public GetChatSessionKeyController(ILogger<GetChatSessionKeyController> logger)
        {
            _logger = logger;
        }



        // GET api/RequestPublicKey/certserial
        [HttpGet("{participatingSerialNumber}")]
        public IActionResult Get(string participatingSerialNumber)
        {

          


            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] GetChat Session Key Called for Serial Numbers : " + participatingSerialNumber);

            // For this they must be logged in, so check if this is initialise or not
            try
            {
                // Go get from auth claims
                ClaimsPrincipal currentUser = this.User;

               


                // As this is the authenticated cert we get a number of claims from the authentication handler
                // issuer thumbprint x500distinguisehedname name serial and dns   
                certSerial = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value;
                string friendlyName = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                string thumbprint = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Thumbprint)?.Value;

                if (certSerial == null)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key No Certificate Serial Number");
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
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key  No Certificate SFriendly Name");
                    return StatusCode(401, "No Friendly Name associated with this certificate");
                }

                //check certificate is one of our registered certificates




                try
                {
                    callerInfo = RegisterNodes.GetClientNode(certSerial, Startup.StaticConfig["Config:clientFileStore"]);

                    // OK -is this a known serial certificate
                    if (string.IsNullOrEmpty(callerInfo.callerID))
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key  Unknown Certificate");
                        return StatusCode(401, "Unknown Certificate");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key  Cannot Identify User");
                    return StatusCode(500, "Cannot identify caller. Exception: " + ex.Message);
                }

                bool isValidCaller = CallerValidateFunction.callerValidate(callerInfo, CallerStatus.requireKemValid);
                // They need a valid KEM key, not a shared secret
                if (!isValidCaller)
                {
                    if (CallerValidateFunction.kemKeyPairNeedsChanging)
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key Kem Key Not Valid");
                        return StatusCode(499, "KemKeyPair Not Valid)");
                    }
                    else
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SChat Session Key  Client Unknown");
                        return StatusCode(401, "Client unknown or invalid");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                return StatusCode(401, "Unable to locate security parameters for client");
            }

            // Request  is all OK so far,
            // but if the KEM private key (or the SharedSecret) are expiring soon, we need to warn the client side now.
            if (CallerValidateFunction.mustIssueKemPrivateKeyExpiryWarning(callerInfo))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key Kem Key Pair Renewal Required");

                return StatusCode(405, "KEM key pair requires renewal before you can proceed");
            }
            if (CallerValidateFunction.mustIssueSharedSecretExpiryWarning(callerInfo))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key Secret Require Renewal");

                return StatusCode(405, "Shared Secret (aka Session Key) requires renewal before you can proceed");
            }

           
            else // Request for shared key
            {


                try
                {
                    // As this is standalone we use bouncy castle

                    Prng getRandom = new Prng();



                    // Set number of iterations for the RFC2898 derivation function
                    // to a reasonably large number, and let's choose a prime number for fun.
                    int iterations = Convert.ToInt16(Startup.StaticConfig["Config:DerivationIterations"]);
                    int saltSize = Convert.ToInt16(Startup.StaticConfig["Config:SaltSize"]);

                    // Get the public key to send


                    //so check it exists

                    try
                    {
                        // Certificate Serial Number
                        if (participatingSerialNumber.Length < 18)
                        {
                            participatingSerialNumber = participatingSerialNumber.PadLeft(18, '0');
                        }

                        participatingPartyInfo = RegisterNodes.GetClientNode(participatingSerialNumber, Startup.StaticConfig["Config:clientFileStore"]);

                        // OK -is this a known serial certificate
                        if (string.IsNullOrEmpty(callerInfo.callerID))
                        {
                            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key  A public key is not held for that  Certificate");
                            return StatusCode(401, "A public key is not held for that  Certificate");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Chat Session Key for requested serial number  Cannot Identify User");
                        return StatusCode(500, "Cannot identify the serial number matching a public key. Exception: " + ex.Message);
                    }


                    // Now get QRNG bytes for the salt

                    byte[] saltBytes = getRandom.GetBytes(saltSize);

                    byte[] chatSessionKey = getRandom.GetBytes(256);

                    int saltsize = saltBytes.Length;

                    // OK - implement the AES encryption
                    AESEncrypt encryptAES = new AESEncrypt();

                    var encryptedBytes1 = encryptAES.Encrypt(chatSessionKey, Convert.FromBase64String(callerInfo.sharedSecretForSession), saltBytes, iterations);

                    //TODO we need to store thios somewhere with the serial numbers of the certificates

                    //add our  header value
                    var sendWithHeader = AESHeaderProcessing.AddEncryptHeader(chatSessionKey.Length, encryptedBytes1);


                    _logger.LogInformation(Convert.ToBase64String(sendWithHeader) + " datasize::" + chatSessionKey.Length);



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
                        string strSalt = Convert.ToBase64String(saltBytes);
                      
                        // string dbgEncKey = Convert.ToBase64String(encryptedBytes1);
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from Chat Session Keycall ");

                        _logger.LogInformation("Debug The public Key encrypted is ::" + Convert.ToBase64String(sendWithHeader) + "::The Public Key in Base64 is ::" + Convert.ToBase64String(chatSessionKey) + "::Saltbytes are ::" + strSalt);
                        return StatusCode(200, "The publicy encrypted is ::" + Convert.ToBase64String(sendWithHeader) + "::The public in Base64 is ::" + Convert.ToBase64String(chatSessionKey) + "::Saltbytes are ::" + strSalt);

                    }
                    else
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from Chat Session Key call ");

                        return StatusCode(200, Convert.ToBase64String(sendWithHeader));
                    }





                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Fail from Chat Session Key::" + ex.Message);

                    return StatusCode(500, "ERROR: Chat Session Key failed with: " + ex.Message);
                }
            }

        }




    }
}
