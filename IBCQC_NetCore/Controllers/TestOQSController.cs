using System;
using System.Text;
using IBCQC_NetCore.OqsdotNet;
using Microsoft.AspNetCore.Mvc;



namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestOQSController : ControllerBase
    {
        // GET: api/<TestOQSController>
        [HttpGet("{algoname}")]
        public IActionResult Get(string algoname)
        {




            using (KEM client = new KEM(algoname))
            {
                Console.WriteLine("Perform key exchange with DEFAULT mechanism");
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

                return StatusCode(500, "Algorithm requested is supported  with: " + supAlgos.ToString());


            }

        }
    }
}
