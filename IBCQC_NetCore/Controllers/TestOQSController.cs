using System;
using System.Text;
using IBCQC_NetCore.OqsdotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestOQSController : ControllerBase
    {


        private readonly ILogger<TestOQSController> _logger;
        private string algoRequested;

        public TestOQSController(ILogger<TestOQSController> logger)
        {
            _logger = logger;
        }


        // GET: api/<TestOQSController>
        [HttpGet("{algoname}")]
        public IActionResult Get(string algoname)
        {

            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] TestOQS called for algorithm: " + algoname);

            //TODO : check if this is  an algo name or an integer

          bool isint =  int.TryParse(algoname, out int algonumber);

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


            using (KEM client = new KEM(algoRequested))
            {
                Console.WriteLine("Perform key exchange with :" + algoRequested);
                StringBuilder supAlgos = new StringBuilder();

                // Print out some info about the mechanism
                supAlgos.AppendLine("Mechanism details:");
                supAlgos.AppendLine(" - Alg name: " + client.AlgorithmName);
                supAlgos.AppendLine(" - Alg version: " + client.AlgorithmVersion);
                supAlgos.AppendLine(" - Claimed NIST level: " + client.ClaimedNistLevel);
                supAlgos.AppendLine(" - Is IND-CCA?: " + client.IsIndCCA);
                supAlgos.AppendLine(" - Secret key length: " + client.SecretKeyLength);
                supAlgos.AppendLine(" - Public key length: " + client.PublicKeyLength);
                supAlgos.AppendLine(" - Ciphertext length: " + client.CiphertextLength);
                supAlgos.AppendLine(" - Shared secret length: " + client.SharedSecretLength);

                // Generate the client's key pair
                byte[] public_key;
                byte[] secret_key;
                client.keypair(out public_key, out secret_key);



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
                    string dbgPrivateKey = "Server not configured to send keys to console"; //Convert.ToBase64String(secret_key);
                    string dbgPublicKey = "Server not configured to send keys to console"; // Convert.ToBase64String(public_key);

                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from TestOQS ");

                    return StatusCode(200, "Algorithm requested is supported with: " + supAlgos.ToString() + "::The private key base64  is ::" + dbgPrivateKey + "::The public key in Base64 is ::" + dbgPublicKey);

                }
                else
                {
                    _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from TestOQS ");
                    return StatusCode(500, "Algorithm requested is supported  with: " + supAlgos.ToString());
                }


            }

        }
    }
}
