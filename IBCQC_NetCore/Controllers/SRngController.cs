
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using IBCQC_NetCore.Encryption;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.Rng;
using static IBCQC_NetCore.Models.ApiEnums;
using System.Security.Claims;



namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SRngController : ControllerBase
    {


        private static CallerInfo callerInfo;
        private static string certSerial;
        private readonly ILogger<SRngController> _logger;

        public SRngController(ILogger<SRngController> logger)
        {
            _logger = logger;
        }


        private int roundUp(int numToRound, int multiple)
        {
            if (multiple == 0)
                return numToRound;

            int remainder = numToRound % multiple;
            if (remainder == 0)
                return numToRound;

            return numToRound + multiple - remainder;
        }


        // GET api/<SRngController>/5
        [HttpGet("{byteCount}")]
        public IActionResult Get(int byteCount)
        {

            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SRng called called with Bytecount of : " +byteCount);

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
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SRNG No Certificate Serial Number");
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
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecret  No Certificate SFriendly Name");
                    return StatusCode(401, "No Friendly Name associated with this certificate");
                }

                //check certificate is one of our registered certificates

              
                try
                {
                   callerInfo = RegisterNodes.GetClientNode(certSerial, Startup.StaticConfig["Config:clientFileStore"]);

                    // OK -is this a known serial certificate
                    if (string.IsNullOrEmpty(callerInfo.callerID))
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecret  Unknown Certificate");
                        return StatusCode(401, "Unknown Certificate");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecret  Cannot Identify User");
                    return StatusCode(500, "Cannot identify caller. Exception: " + ex.Message);
                }

                bool isValidCaller = CallerValidateFunction.callerValidate(callerInfo, CallerStatus.requireKemValid);
                // They need a valid KEM key, not a shared secret
                if (!isValidCaller)
                {
                    if (CallerValidateFunction.kemKeyPairNeedsChanging)
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecret Kem Key Not Valid");
                        return StatusCode(498, "KemKeyPair Not Valid)");
                    }
                    else
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecret  Client Unknown");
                        return StatusCode(401,"Client unknown or invalid");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                return StatusCode(401,"Unable to locate security parameters for client");
            }

            // Request for entropy is all OK so far,
            // but if the KEM private key (or the SharedSecret) are expiring soon, we need to warn the client side now.
            if (CallerValidateFunction.mustIssueKemPrivateKeyExpiryWarning(callerInfo))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecret Kem Key Pair Renewal Required");

                return StatusCode(405, "KEM key pair requires renewal before you can proceed");
            }
            if (CallerValidateFunction.mustIssueSharedSecretExpiryWarning(callerInfo))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecretShared Secret Require Renewal");

                return StatusCode(405, "Shared Secret (aka Session Key) requires renewal before you can proceed");
            }

            if (byteCount == 0) // Encapsulation of a new Private  Key
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SharedSecret  No bytes requested");

                return StatusCode(400,"Nothing requested");
            }
            else // Request for actual QRNG
            {
                // Round up ByteCount so that we get an exact number of 32 byte blocks
              //  byteCount = roundUp(byteCount, 32);

                try
                {
                    // As this is standalone we use bouncy castle

                    Prng getRandom = new Prng();
                    //so get 256 Byte key for AES

                    // Set number of iterations for the RFC2898 derivation function
                    // to a reasonably large number, and let's choose a prime number for fun.
                    int iterations = Convert.ToInt16(Startup.StaticConfig["Config:DerivationIterations"]);
                    int saltSize = Convert.ToInt16(Startup.StaticConfig["Config:SaltSize"]);

                    // Get some bytes to send
                    byte[] bytes1 = new byte[byteCount];
                    bytes1 = getRandom.GetBytes(byteCount);
                    // Now get QRNG bytes for the salt

                    byte[] saltBytes = getRandom.GetBytes(saltSize);
                    int saltsize = saltBytes.Length;

                    // OK - implement the AES mcryption
                    AESEncrypt encryptAES = new AESEncrypt();

                    var encryptedBytes1 = encryptAES.Encrypt(bytes1,Convert.FromBase64String(callerInfo.sharedSecretForSession),saltBytes,iterations);



                    //add our  header value
                    var sendWithHeader = AESHeaderProcessing.AddEncryptHeader(byteCount, encryptedBytes1);




                  

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
                        string dbgPrivateKey = Convert.ToBase64String(bytes1);
                        string dbgEncKey = Convert.ToBase64String(encryptedBytes1);
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from SRNG call ");
                        return StatusCode(200, "The entropy encrypted is ::" + Convert.ToBase64String(sendWithHeader) + "::The entropy in Base64 is ::" + dbgPrivateKey + "::Saltbytes are ::" + strSalt);

                    }
                    else
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from SRNG call ");

                        return StatusCode(200, Convert.ToBase64String(encryptedBytes1));
                    }




                   
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Fail from SRNG::" + ex.Message);

                    return StatusCode(500, "ERROR: SRNG failed with: " + ex.Message);
                }
            }

        }


    }
}
