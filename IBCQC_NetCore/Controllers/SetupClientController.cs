using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhoneNumbers;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.OqsdotNet;
using System.Text.Json;

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



        [HttpPost]
        public IActionResult Post([FromBody] NewClient postedClientInfo)
        {
            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client called");

            //use the supportedalgorithms to look for supported algorithm

            var algoRequested = Enum.GetName(typeof(SupportedAlgorithmsEnum), Convert.ToInt16(postedClientInfo.kemAlgorithm));


            if (String.IsNullOrEmpty(algoRequested))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] SetupClient  Unsupported Algo :::" + algoRequested);
                return StatusCode(400,"Unsupported Algorithm");
            }



            if (String.IsNullOrEmpty(postedClientInfo.clientCertName))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client No Certificate Details");
                return StatusCode(400, "No Cert Details");
            }
            if (String.IsNullOrEmpty(postedClientInfo.clientCertSerialNumber))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client No Certificate Details");
                return StatusCode(400,"No Cert Details");
            }

            if (String.IsNullOrEmpty(postedClientInfo.countryCode))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client Invalid Country Code");
                return StatusCode(400,"Invalid Country Code");
            }
            if (String.IsNullOrEmpty(postedClientInfo.smsNumber))
            {
                // TODO: Then what?
            }
            if (String.IsNullOrEmpty(postedClientInfo.email))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client Invalid Email");
                return StatusCode(400,"No valid Email");
            }
            if (String.IsNullOrEmpty(postedClientInfo.keyparts))
            {
                _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client wrong number of Keyparts");
                return StatusCode(400, "Invalid keyparts");
            }

            else 
            {
              
                

                ValidateEmail xx = new ValidateEmail(_logger);
                bool validEmail = xx.IsValidEmail(postedClientInfo.email);
                if (!validEmail)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client Invalid Email");
                    return StatusCode(400,"Not a valid Email");
                }

                // OK - now we need to know if the certificate is in use
                // Test ensure read write to store is working

               

                if (postedClientInfo.clientCertSerialNumber.Length < 18)
                {
                    postedClientInfo.clientCertSerialNumber = postedClientInfo.clientCertSerialNumber.PadLeft(18, '0');
                }

                if (RegisterNodes.nodeExists(postedClientInfo.clientCertSerialNumber, Startup.StaticConfig["Config:clientFileStore"]))
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Setup Client Certificate Exists");
                    return StatusCode(400,"Client Certificate Already Exists");
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
                    return  StatusCode(400,"ERROR: Is Valid Mobile: " + isMobile + ", Is Valid Region: " + isValidRegion);
                }
              

                int nextid = RegisterNodes.GetNextID(Startup.StaticConfig["Config:clientFileStore"]);



 

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
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Error 500 from Setup Client Call ");
                    return StatusCode(500);
                }

               
                ReturnKeyFormat debugReturnStr = SplitKeyHandlerFunction.SendKeyParts(Convert.ToInt16(postedClientInfo.keyparts),secret_key);

              var newclientinfo =  JsonSerializer.Serialize<ReturnKeyFormat>(debugReturnStr);

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


                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from Setup Client Call ");

                    return StatusCode(200, "The new client return is  ::" + newclientinfo + "::The private key in Base64 is ::" + dbgPrivateKey);

               
                }
                else
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from Setup Client Call ");

                    return Ok(newclientinfo); 
                }






                 

            }


        }
    }
}
