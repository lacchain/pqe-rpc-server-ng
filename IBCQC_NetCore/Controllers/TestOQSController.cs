using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBCQC_NetCore.OqsdotNet;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestOQSController : ControllerBase
    {
        // GET: api/<TestOQSController>
        [HttpGet]
        public IActionResult Get()
        {
            using (KEM client = new KEM("DEFAULT"),
                      server = new KEM("DEFAULT"))
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

              return StatusCode(500, "ERROR: GetKeyPair failed with: " +supAlgos.ToString());


            }

        }
    }
}
