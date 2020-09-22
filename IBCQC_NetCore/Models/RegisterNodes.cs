using IBCQC_NetCore.TempKeys;
using System;
using System.IO;
using System.Text.Json;

namespace IBCQC_NetCore.Models
{
    public class RegisterNodes
    {
        /// <summary>
        /// Read and write to a json file to store node information on certificates and keys
        /// replaces tyhe key storage used in production
        /// </summary>
        /// <returns></returns>
        public AllCallerInfo readNodes(string filename)
        {

            var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
            string jsonString = System.IO.File.ReadAllText(filePath);

            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
            AllCallerInfo allCallerInfo = JsonSerializer.Deserialize<AllCallerInfo>(jsonString);

            return allCallerInfo;

        }

        public bool writeNodes(CallerInfo newNode, string filename)
        {
            var allCallerInfo = readNodes(filename);
            var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
            allCallerInfo.CallerInfo.Add(newNode);
            ////serialize the new updated object to a string
            string towrite = JsonSerializer.Serialize(allCallerInfo);
            ////overwrite the file and it wil contain the new data
            System.IO.File.WriteAllText(filePath, towrite);


            return true;
        }

        public bool nodeExists(string certserial, string filename)
        {
            var allCallerInfo = readNodes(filename);
            foreach (var callerInfo in allCallerInfo.CallerInfo)
            {

                if (callerInfo.clientCertSerialNumber == certserial)
                {
                    return true;

                }


            }






            return false;
        }

        internal static CqcKeyPair GetKemKey(string kemAlgorithm, string filename)
        {
            CqcKeyPair newpair = new CqcKeyPair();

            var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
            string jsonString = System.IO.File.ReadAllText(filePath);

            StoredKemKeys allKemKeys = JsonSerializer.Deserialize<StoredKemKeys>(jsonString);

            //housekeeping keep the file with available keys
            //we set a key to one if used when we get to 10 we reset





            bool availablekeys = false;

            foreach (var keyValues in allKemKeys.KemKeys)
            {
                if (keyValues.keyAlgorithm == kemAlgorithm && keyValues.isUsed == "0")
                {
                    availablekeys = true;
                    break;
                }

            }

            if (!availablekeys)

            {
                //reset the keys
                foreach (var keyValues in allKemKeys.KemKeys)
                {
                    if (keyValues.keyAlgorithm == kemAlgorithm)
                        keyValues.isUsed = "0";

                }

                string towrite = JsonSerializer.Serialize(allKemKeys);
                ////overwrite the file and it wil contain the new data
                System.IO.File.WriteAllText(filePath, towrite);


            }

            //we have updated the file and the object so we have now keys available

            foreach (var keyPair in allKemKeys.KemKeys)
            {




                if (keyPair.isUsed == "0" && keyPair.keyAlgorithm == kemAlgorithm)
                {
                    //looks convoluted but need to stick with what we have to ensure the libraries can slot in when ready


                    newpair.PrivateKey = Convert.FromBase64String(keyPair.kemPrivateKey);
                    newpair.PublicKey = Convert.FromBase64String(keyPair.kemPublicKey);

                    return newpair;

                }




            }

            return null;


        }
    }
}
