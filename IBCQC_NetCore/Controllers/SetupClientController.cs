using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhoneNumbers;

using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.OqsdotNet;

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupClientController : ControllerBase
    {
        //// GET: SetupClientController
        /// <summary>
        ///
        /// </summary>
        /// <param name="postedClientInfo"></param>
        /// <returns></returns>

        //private IAlgorithmServiceManager _algorithmServiceManager;
        private static PhoneNumberUtil _phoneUtil;
        private readonly ILogger<SetupClientController> _logger;

        public SetupClientController(ILogger<SetupClientController> logger)
        {
            _logger = logger;
            _phoneUtil = PhoneNumberUtil.GetInstance();
        }


        //public SetupClientController()//IAlgorithmServiceManager algorithmServiceManager)
        //{
        //   // _algorithmServiceManager = algorithmServiceManager;
           
        //}


        [HttpPost]
        public IActionResult Post([FromBody] NewClient postedClientInfo)
        {
            //// OK, if the posted json fails they will get default message on validatio.
            //// Then default is to just set values to nulls

            if (String.IsNullOrEmpty(postedClientInfo.clientCertName))
            {
                return BadRequest( "No Cert Details");
            }
            if (String.IsNullOrEmpty(postedClientInfo.clientCertSerialNumber))
            {
                return BadRequest("No Cert Details");
            }

            if (String.IsNullOrEmpty(postedClientInfo.countryCode))
            {
                return BadRequest("Invalid Country Code");
            }
            if (String.IsNullOrEmpty(postedClientInfo.smsNumber))
            {
                // TODO: Then what?
            }
            if (String.IsNullOrEmpty(postedClientInfo.email))
            {
                return BadRequest("No valid Email");
            }
            if (String.IsNullOrEmpty(postedClientInfo.keyparts))
            {
                return BadRequest( "Invalid keyparts");
            }

            if (String.IsNullOrEmpty(postedClientInfo.kemAlgorithm))
            {
                return BadRequest("Unknown Algorithm");
            }
            else //check the type is supported
            {
                bool supported = false;
                // Get access to config
                var Conf = Startup.StaticConfig;

                var allalgos = Conf.GetSection("PQEAlgorithms").GetChildren();

                // Read the list of algoritjhms
                foreach (var algo in allalgos)
                {
                    if (algo.Value == postedClientInfo.kemAlgorithm.Trim())
                    {
                        supported = true;
                    }
                }
                if (!supported)
                {
                    return BadRequest( "Unsupported Algorithm");
                }

                ValidateEmail xx = new ValidateEmail(_logger);
                bool validEmail = xx.IsValidEmail(postedClientInfo.email);
                if (!validEmail)
                {
                    return BadRequest("Not a valid Email");
                }

                // OK - now we need to know if the certificate is in use
                // Test ensure read write to store is working

               

                if (postedClientInfo.clientCertSerialNumber.Length < 18)
                {
                    postedClientInfo.clientCertSerialNumber = postedClientInfo.clientCertSerialNumber.PadLeft(18, '0');
                }

                if (RegisterNodes.nodeExists(postedClientInfo.clientCertSerialNumber, Startup.StaticConfig["Config:clientFileStore"]))
                {
                    return BadRequest("Client Certificate Already Exists");
                }

                // Variabels for phone checking
                bool isMobile = false;
                bool isValidNumber = false;
                bool isValidRegion = false;
                string originalNumber;

                //// Check the phone number
                try
                {
                    PhoneNumber phoneNumber = _phoneUtil.Parse(postedClientInfo.smsNumber, postedClientInfo.countryCode);
                    isValidNumber = _phoneUtil.IsValidNumber(phoneNumber);          // Returns true for valid number

                    // Returns true or false w.r.t phone number with the specified region
                    isValidRegion = _phoneUtil.IsValidNumberForRegion(phoneNumber, postedClientInfo.countryCode);
                    string region = _phoneUtil.GetRegionCodeForNumber(phoneNumber); // GB, US , et al

                    var numberType = _phoneUtil.GetNumberType(phoneNumber);         // Produces Mobile , FIXED_LINE
                    string phoneNumberType = numberType.ToString();

                    if (!string.IsNullOrEmpty(phoneNumberType) && phoneNumberType == "MOBILE")
                    {
                       isMobile = true;
                    }
                    originalNumber = _phoneUtil.Format(phoneNumber, PhoneNumberFormat.E164); // Produces "+923336323997"
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                    isMobile = false;
                    isValidRegion = false;
                    return  BadRequest("ERROR: Is Valid Mobile: " + isMobile + ", Is Valid Region: " + isValidRegion);
                }
                //// PhoneNumber is OK





                int nextid = RegisterNodes.GetNextID(Startup.StaticConfig["Config:clientFileStore"]);


                //need the correct name for the algorithm 

                //  var algoRequested = (SupportedAlgorithmsEnum)Convert.ToInt16(callerInfo.kemAlgorithm);

                var algoRequested = Enum.GetName(typeof(SupportedAlgorithmsEnum), Convert.ToInt16(postedClientInfo.kemAlgorithm));

                //because enum has no hypen 
                algoRequested = algoRequested.Replace("_", "-");
                    byte[] public_key;
                    byte[] secret_key;
                using (KEM client = new KEM(algoRequested))
                {

                    // Generate the client's key pair
                   
                    client.keypair(out public_key, out secret_key);

                }

                    CallerInfo storeClient = new CallerInfo();

                storeClient.callerID = nextid.ToString();
                storeClient.kemAlgorithm = postedClientInfo.kemAlgorithm;
                storeClient.kemPublicKey = Convert.ToBase64String(public_key);
                storeClient.keyExpiryDate = DateTime.Now.AddYears(2).ToShortDateString();
                storeClient.clientCertName = postedClientInfo.clientCertName;
                storeClient.clientCertSerialNumber = postedClientInfo.clientCertSerialNumber;
                storeClient.isInitialise = "true";
                storeClient.sharedSecretExpiryDurationInSecs = "7200";
                storeClient.sharedSecretExpiryTime = DateTime.Now.ToShortDateString();
                storeClient.kemPrivateKey = Convert.ToBase64String(secret_key);

                try
                {
                    var whatamI = RegisterNodes.writeNodes(storeClient, Startup.StaticConfig["Config:clientFileStore"]);
                    
                }
                catch
                {
                    return StatusCode(500);
                }

                SplitKeyHandler myHandler = new SplitKeyHandler();
                ReturnKeyFormat debugReturnStr = myHandler.SendKeyParts(Convert.ToInt16(postedClientInfo.keyparts),secret_key);

                return Ok(debugReturnStr);

            }


        }
    }
}
