
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using IBCQC_NetCore.Encryption;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.Rng;
using static IBCQC_NetCore.Models.ApiEnums;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SRngController : ControllerBase
    {


        private CallerInfo callerInfo = new CallerInfo();
     private static string certSerial;

        // private ICqcRng _cqcSRng;
        //  private ISymmetricEncryptionManager _encryptionManager;
        //  private IAlgorithmServiceManager _algorithmServiceManager;
        private readonly ILogger<SRngController> _logger;

        public SRngController(ILogger<SRngController> logger)
        {
            _logger = logger;
        }


        /// <summary>
        ///  This is the default constructor
        /// </summary>
        /// <param name="encryptionManager"></param>
        /// <param name="algorithmServiceManager"></param>
        /// <param name="cqcRng"></param>
        //public SRngController(ISymmetricEncryptionManager encryptionManager, IAlgorithmServiceManager algorithmServiceManager, ICqcRng cqcRng)
        //{
        //    APILogging.Log("SRng", "Constructor");

        //    // Get our services
        //    _cqcSRng = cqcRng;
        //    _encryptionManager = encryptionManager;
        //    _algorithmServiceManager = algorithmServiceManager;
        //}


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
                        return StatusCode(498, "KemKeyPair Not Valid)");
                    }
                    else
                    {
                        return Unauthorized("Client unknown or invalid");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                return Unauthorized("Unable to locate security parameters for client");
            }

            // Request for entropy is all OK so far,
            // but if the KEM private key (or the SharedSecret) are expiring soon, we need to warn the client side now.
            if (CallerValidateFunction.mustIssueKemPrivateKeyExpiryWarning(callerInfo))
            {
                return StatusCode(405, "KEM key pair requires renewal before you can proceed");
            }
            if (CallerValidateFunction.mustIssueSharedSecretExpiryWarning(callerInfo))
            {
                return StatusCode(405, "Shared Secret (aka Session Key) requires renewal before you can proceed");
            }

            if (byteCount == 0) // Encapsulation of a new Private  Key
            {
                return StatusCode(400,"Nothing requested");
            }
            else // Request for actual QRNG
            {
                // Round up ByteCount so that we get an exact number of 32 byte blocks
                byteCount = roundUp(byteCount, 32);

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
                    // Debug check shared secret

                    string b64Shared = callerInfo.sharedSecretForSession;
                    // Send as base64
//#if debug
                    string sendBytes = Convert.ToBase64String(encryptedBytes1);
                    AESDecrypt decryptAES = new AESDecrypt();
                    var decryptbytes = decryptAES.AESDecryptBytes(encryptedBytes1, callerInfo.sharedSecretForSession, saltSize, iterations);
//#endif
                    return Ok(Convert.ToBase64String(encryptedBytes1));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "ERROR: SRNG failed with: " + ex.Message);
                }
            }

        }


    }
}
