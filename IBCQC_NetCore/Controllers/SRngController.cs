using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.Rng;
using Microsoft.AspNetCore.Mvc;
using static IBCQC_NetCore.Models.ApiEnums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SRngController : ControllerBase
    {

        private CallerValidate valCaller = new CallerValidate();
        private CallerInfo callerInfo = new CallerInfo();
        private static string certSerial;

        // private ICqcRng _cqcSRng;
        //  private ISymmetricEncryptionManager _encryptionManager;
        //  private IAlgorithmServiceManager _algorithmServiceManager;




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
                // Cert req if passed validation then we have a client cert
                bool ignoreClientCertificateErrors = Convert.ToBoolean(Startup.StaticConfig["Config:IgnoreClientCertificateErrors"]);
                if (ignoreClientCertificateErrors)
                {
                    return  /*401*/Unauthorized("WARNING: Not supported while Client Certificate checks are disabled");
                }

                var cert = Request.HttpContext.Connection.ClientCertificate;

                // Get the public key
                byte[] userPublicKey = cert.GetPublicKey();

                certSerial = cert.SerialNumber;


                if (certSerial.Length < 18)
                {
                    certSerial = certSerial.PadLeft(18, '0');
                }


                // TODO: Change to GetCCaller(userPublicKey)


                RegisterNodes chkNode = new RegisterNodes();
                try
                {
                    callerInfo = chkNode.GetClientNode(certSerial, "RegisteredUsers.json");

                    //ok now to crteate the key parts
                    if (string.IsNullOrEmpty(callerInfo.callerID))
                    {

                        return Unauthorized("Unknown Certificate");
                    }
                }

                catch (Exception ex)
                {
                    return StatusCode(500, "Cannot identify caller.");
                }



                bool isValidCaller = valCaller.callerValidate(callerInfo, CallerStatus.requireKemValid);
                // They need a valid KEM key, not a shared secret
                if (!isValidCaller)
                {
                    if (valCaller.kemKeyPairNeedsChanging)
                    {

                        return StatusCode(498, "KemKeyPair Not Valid)");// Content((System.Net.HttpStatusCode)498 /*TokenExpiredOrInvalid*/, "KEM KeyPair not valid");
                    }
                    else
                    {

                        return Unauthorized("Client unknown or invalid");
                    }
                }
            }
            catch (Exception ex)
            {

                return Unauthorized("Unable to locate security parameters for client");
            }


            // Request for entropy is all OK so far,
            // but if the KEM private key (or the SharedSecret) are expiring soon, we need to warn the client side now.
            if (valCaller.mustIssueKemPrivateKeyExpiryWarning(callerInfo))
            {
                return StatusCode(405, "KEM key pair requires renewal before you can proceed");
            }
            if (valCaller.mustIssueSharedSecretExpiryWarning(callerInfo))
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


                    //as this is standalone we use bouncy castle

                    Prng getRandom = new Prng();
                    //so get 256 Byte key for AES 



                    var randomBytes = getRandom.GetBytes(256);

                    // Get some bytes for the shared key
                    byte[] bytes1 = new byte[byteCount];
                    
                    bytes1 = getRandom.GetBytes(byteCount);
                    // Now get QRNG bytes for the salt
                  
                    byte[] saltBytes = getRandom.GetBytes(16);
                    int saltsize = saltBytes.Length;

                    // Set number of iterations for the RFC2898 derivation function
                    // to a reasonably large number, and let's choose a prime number for fun.
                    int iterations = 11113;

                    //ok implement the AES mcryption
                    //var encryptedBytes1 = _encryptionManager.Encrypt_UsingKeyBytes(bytes1,
                    //                                                 Convert.FromBase64String(callerInfo.sharedSecretForSession),
                    //                                                 saltBytes,
                    //                                                 iterations);
                    // Debug check shared secret
                 
                    string b64Shared = callerInfo.sharedSecretForSession;

                    // Send as base64

                    //  string sendBytes = Convert.ToBase64String(encryptedBytes1);

                    return Ok("");//    sendBytes);
                }
                catch (Exception ex)
                {
                   
                    return StatusCode(500, "ERROR: SRNG failed with: " + ex.Message);
                }
            }

           

        }

      
    }
}
