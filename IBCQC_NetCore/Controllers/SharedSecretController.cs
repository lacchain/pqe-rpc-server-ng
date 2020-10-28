using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.OqsdotNet;
using IBCQC_NetCore.Rng;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using static IBCQC_NetCore.Models.ApiEnums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedSecretController : ControllerBase
    {

        private CallerValidate valCaller = new CallerValidate();
        private CallerInfo callerInfo = new CallerInfo();
        private static string certSerial;


        // GET: api/<SharedSecretController>
        [HttpGet]
        public IActionResult Get()
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

                bool isValidCaller = valCaller.callerValidate(callerInfo, CallerStatus.requireKemValid);
                // They need a valid KEM key, not a shared secret
                if (!isValidCaller)
                {
                    if (valCaller.kemKeyPairNeedsChanging)
                    {
                        return StatusCode(498,"KemKeyPair Not Valid)");// Content((System.Net.HttpStatusCode)498 /*TokenExpiredOrInvalid*/, "KEM KeyPair not valid");
                    }
                    else
                    {
                        return Unauthorized( "Client unknown or invalid");
                    }
                }
            }
            catch(Exception ex)
            {
                return Unauthorized("Unable to locate security parameters for client. Exception: " + ex.Message);
            }

            // Request for a new SharedSecret is all OK so far,
            // but if the private key is expiring soon, we need to warn the client side now.
            if (valCaller.mustIssueKemPrivateKeyExpiryWarning(callerInfo))
            {
                return StatusCode(405, "KEM key requires renewal before you can proceed");
            }

            // OK. Let's create a shared secret
            // as this is standalone we use bouncy castle

            Prng getRandom = new Prng();
            // So get 256 Byte key for AES

            var randomBytes = getRandom.GetBytes(256);
            try
            {

                var algoRequested = Enum.GetName(typeof(SupportedAlgorithmsEnum), Convert.ToInt16(callerInfo.kemAlgorithm));

                //because enum has no hypen 
                algoRequested = algoRequested.Replace("_", "-");

                var public_key =Convert.FromBase64String(callerInfo.kemPublicKey);

                using (KEM client = new KEM(algoRequested))
                            {

                    byte[] ciphertext;
                    byte[] shared_secret;

                    client.encaps(out ciphertext, out shared_secret, public_key); 
                     
                    string ciphertextB64 = Convert.ToBase64String(ciphertext);
                    
                    var updSecret = RegisterNodes.UpdSharedSecret(ciphertextB64, Startup.StaticConfig["Config:clientFileStore"],certSerial);

                            return Ok(ciphertextB64); 
                
                }

       
                    
            }
            catch (Exception ex)
            {
                return StatusCode(500, "ERROR: GetSharedSecret failed with: " + ex.Message);
            }
        }

    }
}
