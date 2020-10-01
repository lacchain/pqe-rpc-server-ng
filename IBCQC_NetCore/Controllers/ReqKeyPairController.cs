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

            RegisterNodes chkNode = new RegisterNodes();
            try
            {
                 callerInfo = chkNode.GetClientNode(certSerial, "RegisteredUsers.json");

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

                //used for testing the fiel nbased stored keys

               


                switch (Convert.ToInt16(callerInfo.kemAlgorithm))
                {
                    case 222: // Frodo Kem640
                              // Generate a new key pair and encrypt with our existing shared secret

                        //FrodoParams frodoId = FrodoParams.Kem640;

                        //// Let us first get a new keypair

                        //var keyPair = _algorithmServiceManager
                        //                .KeyEncapsulationService<FrodoKemService, FrodoParams>(frodoId)
                        //                .KeyGen();


                        //for testing we use the file containing fixed keys

                        CqcKeyPair cqcKeyPair = RegisterNodes.GetKemKey(callerInfo.kemAlgorithm, "TempKeys.json");

                        //getcallersql.SetPublicKey(callerInfo.callerId,
                        //                          keyPair.PublicKey,
                        //                          keyPair.PrivateKey,
                        //                          false);



                        // Now use AES




                        encryptedBytes1 = encryptAES.Encrypt(cqcKeyPair.PrivateKey,
                                                                     Convert.FromBase64String(callerInfo.sharedSecretForSession),
                                                                     saltBytes,
                                                                     iterations);

                        // Send as base64
                       
                        sendBytes = Convert.ToBase64String(encryptedBytes1);
                        
                        return StatusCode(200,sendBytes);

                    case 322: // McEliece6960119

                        //  McElieceParams McElId = IronBridge.Models.Encapsulation.McElieceParams.McEliece6960119;

                        // First, let us generate a new keypair

                        //var mcKeyPair = _algorithmServiceManager
                        //                  .KeyEncapsulationService<McElieceService, McElieceParams>(McElId)
                        //                  .KeyGen();

                        // OK - Now we want to ecapsulate the private key with their old public key

                        // This is a new call and extended in McEliece6960119 service to not return the key in plain text
                        //var McEncapsulation = _algorithmServiceManager
                        //                      .KeyEncapsulationService <McElieceService, McElieceParams>(McElId)
                        //                      .Encapsulate(callerInfo.kemPublicKey, mcKeyPair.PrivateKey, true);


                        //for testing we use the file containing fixed keys


                        CqcKeyPair mcCqcKeyPair = RegisterNodes.GetKemKey(callerInfo.kemAlgorithm, "TempKeys.json");

                        //getcallersql.SetPublicKey(callerInfo.callerId,
                        //                          mcKeyPair.PublicKey,
                        //                          mcKeyPair.PrivateKey,
                        //                          false);




                        // Now use AES

                        // OK - implement the AES mcryption




                        encryptedBytes1 = encryptAES.Encrypt(mcCqcKeyPair.PrivateKey,
                                                                     Convert.FromBase64String(callerInfo.sharedSecretForSession),
                                                                     saltBytes,
                                                                     iterations);


                        // Send as base64

                        sendBytes = Convert.ToBase64String(encryptedBytes1);
                      
                        return StatusCode(200,sendBytes);

                    default:
                       
                        return StatusCode(400, "Unsupported KEM Algorithm");
                }
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "ERROR: GetKeyPair failed with: " + ex.Message);
            }


  







        }






  
    }
}
