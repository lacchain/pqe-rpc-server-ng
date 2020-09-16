using IBCQC_NetCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("setupclient")]
    [ApiController]
    public class SetupClientController : ControllerBase
    {
        //// GET: SetupClientController
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postedClientInfo"></param>
        /// <returns></returns>

      //  private IAlgorithmServiceManager _algorithmServiceManager;
        private static PhoneNumberUtil _phoneUtil;
      


        public SetupClientController()//IAlgorithmServiceManager algorithmServiceManager)
        {
           // _algorithmServiceManager = algorithmServiceManager;
            _phoneUtil = PhoneNumberUtil.GetInstance();
        }


        [HttpPost]
        public string Post([FromBody] NewClient postedClientInfo)
        {
            ////ok if the posted json fails they will get default message on validatio.
            //// Then default is to just set values to nulls 



            if (String.IsNullOrEmpty(postedClientInfo.clientCertName))
            {

                return "No Cert Details";

            }
            if (String.IsNullOrEmpty(postedClientInfo.clientCertSerialNumber))
            {

                return "No Cert Details";
            }

            if (String.IsNullOrEmpty(postedClientInfo.countryCode))
            {
                return "Invalid Country Code";

            }
            if (String.IsNullOrEmpty(postedClientInfo.smsNumber))
            {


            }
            if (String.IsNullOrEmpty(postedClientInfo.email))
            {
                return "No valid Email";

            }
            if (String.IsNullOrEmpty(postedClientInfo.keyparts))
            {
                return "Invalid keyparts";

            }

            if (String.IsNullOrEmpty(postedClientInfo.kemAlgorithm))
            {

                return "Unknown Algorithm";
            }
            else //check the type is supported
            {

                bool supported = false;


                //get acces to config
                var Conf = Startup.StaticConfig;

                var allalgos = Conf.GetSection("PQEAlgorithms").GetChildren();


                //read the list of algoritjhms 
                foreach (var algo in allalgos)
                {

                    if (algo.Value == postedClientInfo.kemAlgorithm.Trim())
                    {
                        supported = true;
                    }


                }
                if (!supported)
                {

                    return "Unsupported Algorithm";
                }

                

                bool validEmail = ValidateEmail.IsValidEmail(postedClientInfo.email);

                if (!validEmail)
                {
                    return "Not a valid Email";
                }



                //ok now we need to know if the certificate is in use 

                //test ensure read write to store is working

                RegistertNodes chkNode = new RegistertNodes();


                if (chkNode.nodeExists(postedClientInfo.clientCertSerialNumber))
                    {
                        return "Client Certificate Already Exists";
                    }

                //variabels for phone checking

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
                    isMobile = false;
                    isValidRegion = false;
                    return "ERROR: Is Valid Mobile: " + isMobile + ": Is Valid Region: " + isValidRegion; //Content(HttpStatusCode.BadRequest /*400*/, "ERROR: Is Valid Mobile: " + isMobile + ": Is Valid Region: " + isValidRegion);
                }
                //// PhoneNumber is OK

                // Get a frodo key kem 640 for AES-128
                //FrodoParams frodoId = FrodoParams.Kem640;

                //// Generate a KEM keypair
                //var keyPair = _algorithmServiceManager
                //                .KeyEncapsulationService<FrodoKemService, FrodoParams>(frodoId)
                //                .KeyGen();

                // OK - Store client info
                CallerInfo storeClient = new CallerInfo();

                storeClient.kemAlgorithm = postedClientInfo.kemAlgorithm;
                storeClient.kemPublicKey = "hfgadhgl;adjhsjhh"; //keyPair.PublicKey;
                storeClient.keyExpiryDate = DateTime.Now.AddYears(2).ToShortDateString();
                storeClient.clientCertName = postedClientInfo.clientCertName;
                storeClient.clientCertSerialNumber = postedClientInfo.clientCertSerialNumber;
                storeClient.isInitialise = "true";
                storeClient.sharedSecretExpiryDurationInSecs = "7200";
                storeClient.sharedSecretExpiryTime = DateTime.Now.ToShortTimeString();
                storeClient.kemPrivateKey = "gjhaergjdrrgjsdljhgstjthsrt"; //keyPair.PrivateKey;


                var whatamI = chkNode.writeNodes(storeClient);

                return "you are at setup client";

            }



        }
    }
}