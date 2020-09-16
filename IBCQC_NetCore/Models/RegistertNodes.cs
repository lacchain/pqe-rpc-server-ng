using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Models
{
    public  class RegistertNodes
    {
        /// <summary>
        /// Read and write to a json file to store node information on certificates and keys
        /// replaces tyhe key storage used in production
        /// </summary>
        /// <returns></returns>
        public AllCallerInfo  readNodes()
        {

            var filePath = Path.Combine(System.AppContext.BaseDirectory, "RegisteredUsers.json");
            string jsonString = System.IO.File.ReadAllText(filePath);

            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
            AllCallerInfo allCallerInfo = JsonSerializer.Deserialize<AllCallerInfo>(jsonString);

            return allCallerInfo;

        }

        public bool writeNodes(CallerInfo newNode)
        {
            var allCallerInfo = readNodes();
            var filePath = Path.Combine(System.AppContext.BaseDirectory, "RegisteredUsers.json");
            allCallerInfo.CallerInfo.Add(newNode);
            ////serialize the new updated object to a string
            string towrite = JsonSerializer.Serialize(allCallerInfo);
            ////overwrite the file and it wil contain the new data
           System.IO.File.WriteAllText(filePath, towrite);


            return true; 
        }

        public bool nodeExists(string certserial)
        {
            var allCallerInfo = readNodes();
            foreach(var callerInfo in allCallerInfo.CallerInfo)
            {

                if (callerInfo.clientCertSerialNumber == certserial)
                {
                    return true;
                
                }
                
                
             }






            return false;
        }


    }
}
