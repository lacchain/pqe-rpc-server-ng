using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IBCQC_NetCore.Models;
using static IBCQC_NetCore.Models.ApiEnums;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Rng;
using IBCQC_NetCore.Encryption;
using Microsoft.AspNetCore.Authorization;
using IBCQC_NetCore.OqsdotNet;
using System.Net.WebSockets;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReqKeyPairController : ControllerBase
    {

        private static CallerInfo callerInfo;
      
        private CallerValidate valCaller = new CallerValidate();




        // GET: api/<ReqKeyPairController>
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            // Go get from auth claims
            ClaimsPrincipal currentUser = this.User;

            // As this is the authenticated cert we get a number of claims from the authentication handler
            // issuer thumbprint x500distinguisehedname name serial and dns   
            string certSerial = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value;
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
                 callerInfo =RegisterNodes.GetClientNode(certSerial, Startup.StaticConfig["Config:clientFileStore"]);

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

          

            // First check if this is intialise or not.
            // If initialise then they would fail the validation checks
            if (!Convert.ToBoolean(callerInfo.isInitialise))
            {
                bool isValidCaller = valCaller.callerValidate(callerInfo, CallerStatus.requireSharedValid);
                if (!isValidCaller)
                {
                    if (valCaller.sharedSecretNeedsChanging)
                    {
                       
                        return StatusCode(498, "SharedSecret has expired");
                    }
                    else
                    {
                       
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

              //  var algoRequested = (SupportedAlgorithmsEnum)Convert.ToInt16(callerInfo.kemAlgorithm);

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

                   
                     
                       return StatusCode(200,sendBytes);

                }




                //for testing we use the file containing fixed keys

             //   CqcKeyPair cqcKeyPair = RegisterNodes.GetKemKey(callerInfo.kemAlgorithm, Startup.StaticConfig["Config:keyFileStore"]);

                      
                        
                        
                   

                  
            }
            catch (Exception ex)
            {
                return StatusCode(400, "Unsupported KEM Algorithm" + ex.Message);
            }


  







        }






  
    }
}
