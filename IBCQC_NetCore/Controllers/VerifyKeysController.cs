
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using IBCQC_NetCore.Functions;
using IBCQC_NetCore.Models;
using IBCQC_NetCore.Rng;
using static IBCQC_NetCore.Models.ApiEnums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifyKeysController : ControllerBase
    {

        //private ICqcRng _cqcSRng;
        //private ISymmetricEncryptionManager _encryptionManager;
        //private IAlgorithmServiceManager _algorithmServiceManager;
        private string certSerial;
        private static CallerInfo callerInfo;
        private CallerValidate valCaller = new CallerValidate();
        private readonly ILogger<VerifyKeysController> _logger;

        public VerifyKeysController(ILogger<VerifyKeysController> logger)
        {
            _logger = logger;
        }


        //public VerifyKeysController(ISymmetricEncryptionManager encryptionManager, IAlgorithmServiceManager algorithmServiceManager, ICqcRng cqcRng)
        //{
        //    _cqcSRng = cqcRng;
        //    _encryptionManager = encryptionManager;
        //    _algorithmServiceManager = algorithmServiceManager;
        //}
        public IActionResult Post([FromBody] KeyValidate keyValidate)
        {
            try
            {
                //var keyValidate = JsonConvert.DeserializeObject<KeyValidate>(reqData);
                //string typeOfKey = keyValidate.typeOfKey;
                //string content = keyValidate.keyToValidate.Replace("\"", string.Empty);
                //string content3 = content.Trim('"');
                //string keyToValidate = content3;

                // Is this the request or response
                string requestType = keyValidate.requestType;

                // Cert req if passed validation then we have a client cert
                bool ignoreClientCertificateErrors = Convert.ToBoolean(Startup.StaticConfig["Config:IgnoreClientCertificateErrors"]);
                if (ignoreClientCertificateErrors)
                {
                    return StatusCode(401,"WARNING: Not supported while Client Certificate checks are disabled");
                }

                var cert = Request.HttpContext.Connection.ClientCertificate;
                byte[] userPublicKey = cert.GetPublicKey();  // Get the public key
                certSerial = cert.SerialNumber;
                if (certSerial.Length < 18)
                {
                    certSerial = certSerial.PadLeft(18, '0');
                }

                // TODO: Change to GetCCaller(userPublicKey)

                RegisterNodes chkNode = new RegisterNodes();
                try
                {
                    callerInfo = chkNode.GetClientNode(certSerial, "RegisteredUsers.json");

                    // OK - now to crteate the key parts
                    if (string.IsNullOrEmpty(callerInfo.callerID))
                    {
                        return Unauthorized("Unknown Certificate");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "VerifyKeys cannot identify caller. Exception: " + ex.Message);
                }

                bool isValidCaller = valCaller.callerValidate(callerInfo, CallerStatus.requireKemValid);
                // They need a valid KEM key, not a shared secret
                if (!isValidCaller)
                {
                    if (valCaller.kemKeyPairNeedsChanging)
                    {
                        return StatusCode(498, "KemKeyPair Not Valid)");// Content((System.Net.HttpStatusCode)498 /*TokenExpiredOrInvalid*/, "KEM KeyPair not valid");
                    }
                    else
                    {
                        return Unauthorized("Client unknown or invalid");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                return Unauthorized("Unable to locate security parameters for client");
            }

            try
            {
                Prng prng = new Prng();

                switch (keyValidate.typeOfKey.ToLower())
                {
                    // For a private key validation we need a valid public key pair.
                    // We encode a random string. They will decode and re-encode with the private key and resubmit
                    case "kem":
                    {
                        if (valCaller.callerValidate(callerInfo, CallerStatus.requireKemValid))
                        {
                            // Check if this is the request or response
                            if (keyValidate.requestType.ToLower() == "response")
                            {
                                // TODO: Get the stored byte array from the file  decapsulate what was sent back and do a byte compare
                                //  KeyValidate validInfo = getcallersql.GetKeyValidationData(callerInfo.callerId);

                                // Get the byte array
                                var respBytes = Convert.FromBase64String(keyValidate.keyToValidate);

                                // OK - Decapsulate
                                switch (Convert.ToInt16(callerInfo.kemAlgorithm))
                                {
                                    case 222: // TODO: Fix magic number
                                    {
                                        //   FrodoParams frodoId = FrodoParams.Kem640;

                                        // Decap with public key as this should be encapsulated with Private Key

                                        //var decapsulation = _algorithmServiceManager
                                        //                    .KeyEncapsulationService<FrodoKemService, FrodoParams>(frodoId)
                                        //                    .Decapsulate(callerInfo.kemPublicKey, respBytes);

                                        ByteCompare compareData = new ByteCompare();
                                        //var isCorrect = (compareData.ByteArrayCompare(decapsulation, Convert.FromBase64String(validInfo.storedData)));

                                        // Delete from the DB
                                        //getcallersql.DeleteKeyValidationData(callerInfo.callerId);

                                        //if (isCorrect)
                                        //{
                                        //    return Json("Valid");
                                        //}
                                        return StatusCode(401,"Invalid");
                                    }

                                    case 322: // TODO: Fix magic number
                                    {
                                        // McElieceParams elieceParams = McElieceParams.McEliece6960119;

                                        // Returns a random encapsulated string
                                        //var mcDecapsulation = _algorithmServiceManager
                                        //                        .KeyEncapsulationService<McElieceService, McElieceParams>(elieceParams)
                                        //                        .Decapsulate(callerInfo.kemPublicKey, respBytes);

                                        ByteCompare mcCompareData = new ByteCompare();

                                        //var isMcCorrect = (mcCompareData.ByteArrayCompare(mcDecapsulation, Convert.FromBase64String(validInfo.storedData)));

                                        // Delete from the DB
                                        //getcallersql.DeleteKeyValidationData(callerInfo.callerId);
                                        //if (isMcCorrect)
                                        //{
                                        //    return Ok("Valid");
                                        //}

                                        return StatusCode(401,"Invalid");
                                    }
                                    default:
                                        return StatusCode(400, "Unsupported KEM algorithm");
                                }
                            }
                            else if (keyValidate.requestType.ToLower() == "request")
                            {
                                // Get a random string and encapsulate with the public key
                                // Store the string in a DB await the response
                                // Allow five minutes only for the respone for now
                                switch (Convert.ToInt16(callerInfo.kemAlgorithm))
                                {
                                    case 222: // TODO: Fix magic number
                                    {
                                        // TODO: Fix magic number
                                        //   FrodoParams frodoId = FrodoParams.Kem640;

                                        // Returns the new Randon String in bytes and the encapsulated version
                                        // Debug logout the actual public key we are using
                                        // Debug use only

                                        //var encapsulation = _algorithmServiceManager
                                        //                    .KeyEncapsulationService<FrodoKemService, FrodoParams>(frodoId)
                                        //                    .Encapsulate(callerInfo.kemPublicKey);

                                        // Store the sent random string
                                        //  getcallersql.SetKeyValidationData(callerInfo.callerId, Convert.ToBase64String(encapsulation.SharedSecret), typeOfKey);

                                        //var b64Cipher = Convert.ToBase64String(encapsulation.CipherText);

                                        return Ok(); // (b64Cipher);
                                    }

                                    case 322: // TODO: Fix magic number
                                    {
                                        // McElieceParams elieceParams = McElieceParams.McEliece6960119;

                                        // Returns a random encapsulated string
                                        //var mcEncapsulation = _algorithmServiceManager
                                        //                        .KeyEncapsulationService<McElieceService, McElieceParams>(elieceParams)
                                        //                        .Encapsulate(callerInfo.kemPublicKey);
                                        //
                                        //    string mcCiphertextB64 = Convert.ToBase64String(mcEncapsulation.CipherText);

                                        // Update the shared secret
                                        //   getcallersql.SetKeyValidationData(callerInfo.callerId, Convert.ToBase64String(mcEncapsulation.SharedSecret), typeOfKey);
                                        //     var mcSecB64 = Convert.ToBase64String(mcEncapsulation.CipherText);
                                        return Ok();// (mcCiphertextB64);
                                    }
                                    default:
                                        return StatusCode(400, "Unsupported KEM algorithm");
                                }
                            }
                            else
                            {
                                return StatusCode(400, "Invalid Request");
                            }
                        }
                        else
                        {
                            if (valCaller.kemKeyPairNeedsChanging)
                            {
                                return StatusCode(498, "KEM KeyPair not valid");
                            }
                            else if (valCaller.sharedSecretNeedsChanging)
                            {
                                return StatusCode(498 , "SharedSecret not valid");
                            }
                            else
                            {
                                return StatusCode(401, "No Valid KeyPair");
                            }
                        }
                    }
                    case "aes":
                    {
                        // For AES we need a shared secret test,
                        // so we will send a code encoded with their shared secret

                        // Now get QRNG bytes for the salt
                        byte[] saltBytes = prng.GetBytes(16);
                        int saltsize = saltBytes.Length;

                        // Set the number of iterations for the RFC2898 derivation function
                        //int RFC2898DeriveBytesIterations = 11113;

                        // Byte array to hold encrypted data
                        //byte[] encryptedBytes;

                        if (valCaller.callerValidate(callerInfo, CallerStatus.requireSharedValid))
                        {
                            // Check if this is the request or response
                            if (keyValidate.requestType.ToLower() == "response")
                            {
                                // Get the byte array
                                var respBytes = Convert.FromBase64String(keyValidate.keyToValidate);

                                // Decrypt the response
                                //var aesDecryptedBytes = _encryptionManager.Decrypt_UsingKeyBytes(respBytes,
                                //                                                    callerInfo.sharedSecretForSession,
                                //                                                    16,
                                //                                                    RFC2898DeriveBytesIterations);

                                // Get the stored random data
                                //var aesValidInfo = getcallersql.GetKeyValidationData(callerInfo.callerId);

                                ByteCompare AesCompareData = new ByteCompare();

                                //var storedDataB64 = Convert.FromBase64String(aesValidInfo.storedData);
                                //var isCorrect = (AesCompareData.ByteArrayCompare(aesDecryptedBytes, storedDataB64));

                                // Delete from the DB
                                //getcallersql.DeleteKeyValidationData(callerInfo.callerId);

                                //if (isCorrect)
                                //{
                                //    return Ok("Valid"); // Compare OK
                                //}
                                // Compare NG

                                return StatusCode(406, "Verify failed");
                            }
                            else if (keyValidate.requestType.ToLower() == "request")
                            {
                                // Request then we grab the random data and encrypt it

                                // Get a random byte array
                                byte[] randomBytes = prng.GetBytes(32);
                                var randomBytesB64 = Convert.ToBase64String(randomBytes);

                                // Encrypt some data
                                //encryptedBytes = _encryptionManager.Encrypt_UsingKeyBytes(randomBytes,
                                //                                            callerInfo.sharedSecretForSession,
                                //                                            saltBytes,
                                //                                            RFC2898DeriveBytesIterations);

                                //var aesDecryptedBytes = _encryptionManager.Decrypt_UsingKeyBytes(encryptedBytes,
                                //                                            callerInfo.sharedSecretForSession,
                                //                                            16,
                                //                                            RFC2898DeriveBytesIterations);

                                //// Store the data in the DB
                                //// Store the sent random string
                                //getcallersql.SetKeyValidationData(callerInfo.callerId, randomBytesB64, typeOfKey);

                                //var encryptedBytesB64 = Convert.ToBase64String(encryptedBytes);

                                return Ok();// (encryptedBytesB64);
                            }
                            else
                            {
                                return StatusCode(400, "VerifyKeys failed. Invalid Request");
                            }
                            // TODO: Add send a code here
                        }
                        else
                        {
                            if (valCaller.kemKeyPairNeedsChanging)
                            {
                                return StatusCode(498 , "KEM KeyPair not valid");
                            }
                            else if (valCaller.sharedSecretNeedsChanging)
                            {
                                return StatusCode(498 , "SharedSecret not valid");
                            }
                            else
                            {
                                return StatusCode(401, "VerifyKeys failed. No Valid Key");
                            }
                        }
                    }
                    default:
                        return StatusCode(400, "VerifyKeys failed. Unsupported key type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ERROR: Failed with exception: " + ex.Message);
                return StatusCode(500, "VerifyKeys failed with exception: " + ex.Message);
            }
        }


    }
}
