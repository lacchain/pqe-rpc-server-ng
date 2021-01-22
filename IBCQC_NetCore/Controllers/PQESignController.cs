using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.OqsdotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PQESignController : ControllerBase
    {
        private static CallerInfo callerInfo;
        private static CallerInfo participatingPartyInfo;
        private static string certSerial;
        private readonly ILogger<PQESignController> _logger;
        private string algoRequested;

        public PQESignController(ILogger<PQESignController> logger)
        {
            _logger = logger;
        }

        //in thbis call we get the serial of the person we invite to chat we get the clients form their certificate

        [HttpGet("{algoname}")]
        public IActionResult Get(string algoname)
        {

            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] TestOQS called for algorithm: " + algoname);

            //TODO : check if this is  an algo name or an integer

            bool isint = int.TryParse(algoname, out int algonumber);

            if (isint)
            {

                //get the algoname

                algoRequested = Enum.GetName(typeof(SupportedAlgorithmsEnum), algonumber);

                //because enum has no hypen 
                algoRequested = algoRequested.Replace("_", "-");

            }

            else
            {
                try
                {
                    //see if it is supported
                    var isConfiged = (SupportedAlgorithmsEnum)System.Enum.Parse(typeof(SupportedAlgorithmsEnum), algoname);
                    algoRequested = isConfiged.ToString();

                    //need to chexck for sign algos except falcon
                    algoRequested = algoRequested.Replace("_", "-");
                }

                catch
                {
                    return StatusCode(500, "Algorithm requested is not supported or recognised : " + algoname);
                }


            }

            if (string.IsNullOrEmpty(algoRequested))
            {
                return StatusCode(500, "Algorithm requested is not supported or recognised : " + algoname);

            }


            Sig signer = new Sig(algoRequested);
            // The message to sign
            byte[] message = new System.Text.UTF8Encoding().GetBytes("Message to sign for validation");

            // Generate the signer's key pair
            byte[] public_key;
            byte[] secret_key;
            signer.keypair(out public_key, out secret_key);

            // The signer sends the public key to the verifier

            // The signer signs the message
            byte[] signature;
            signer.sign(out signature, message, secret_key);


            
            SplitKeyHandlerFunction.ByteToHexBitFiddle(message);

            // The signer sends the signature to the verifier
            //get nw instanmce to do verification

            var verifier = new Sig(algoRequested);


            // The verifier verifies the signature
            if (verifier.verify(message, signature, public_key))
            {
                return StatusCode(200, "Algorithm requested is supported : " + algoname +":: Signature ::" + SplitKeyHandlerFunction.ByteToHexBitFiddle(signature) +":: Message::" + SplitKeyHandlerFunction.ByteToHexBitFiddle(message) + ":: Public Key::" + SplitKeyHandlerFunction.ByteToHexBitFiddle(public_key));
            }


            return StatusCode(200, "Failed to verify but Algorithm requested is supported : " + algoname);


        }

        }
}
