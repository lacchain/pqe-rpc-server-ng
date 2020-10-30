
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
        internal static AllCallerInfo readNodes(string filename)
        {
            var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);

            if (!System.IO.File.Exists(filePath))
            {
                //return null;
                Console.WriteLine("ERROR: readNodes failed. File not found: " + filePath);
                throw new FileNotFoundException("ERROR: readNodes failed. File not found: " + filePath);
            }
            string jsonString = System.IO.File.ReadAllText(filePath);

            AllCallerInfo allCallerInfo = JsonSerializer.Deserialize<AllCallerInfo>(jsonString);
            if (allCallerInfo.CallerInfo == null)
            {
                //return null;
                Console.WriteLine("ERROR: readNodes failed to parse: " + filePath);
                throw new FormatException("ERROR: readNodes failed to parse: " + filePath);
            }
            return allCallerInfo;
        }

        internal static bool writeNodes(CallerInfo newNode, string filename)
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

        internal static bool nodeExists(string certserial, string filename)
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





        internal static CallerInfo GetClientNode(string serialNumber, string filename)
        {
            var allCallerInfo = readNodes(filename);
            foreach (var callerInfo in allCallerInfo.CallerInfo)
            {
                if (callerInfo.clientCertSerialNumber.ToLower() == serialNumber.ToLower())
                {
                    return callerInfo;
                }
            }

            return new CallerInfo();
        }



        internal static bool UpdKemPublicKey(string public_Key, string filename, string serialNumber)
        {
            try
            {
                var allCallerInfo = readNodes(filename);

                // For writing back
                var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
                foreach (var callerInfo in allCallerInfo.CallerInfo)
                {

                    if (callerInfo.clientCertSerialNumber.ToLower() == serialNumber.ToLower())
                    {
                        callerInfo.kemPublicKey = public_Key;
                        callerInfo.keyExpiryDate= DateTime.Now.AddYears(5).ToShortDateString();
                        ////serialize the new updated object to a string
                        string towrite = JsonSerializer.Serialize(allCallerInfo);
                        ////overwrite the file and it wil contain the new data
                        System.IO.File.WriteAllText(filePath, towrite);
                        return true;
                    }




                }
                return false;
            }
            catch
            {
                return false;
            }

        }


        internal static bool UpdSharedSecret(string sharedsecret, string filename, string serialNumber)
        {
            try
            {
                var allCallerInfo = readNodes(filename);

                // For writing back
                var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
                foreach (var callerInfo in allCallerInfo.CallerInfo)
                {
                    
                    if (callerInfo.clientCertSerialNumber.ToLower() == serialNumber.ToLower())
                    {
                        callerInfo.sharedSecretForSession = sharedsecret;
                        callerInfo.sharedSecretExpiryTime = DateTime.Now.ToLongDateString();
                        ////serialize the new updated object to a string
                        string towrite = JsonSerializer.Serialize(allCallerInfo);
                        ////overwrite the file and it wil contain the new data
                        System.IO.File.WriteAllText(filePath, towrite);
                        return true;
                    }




                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        internal static  int GetNextID(string filename)
        {
            int nextid = 0;

          
           
            var allCallerInfo = readNodes(filename);

            foreach (var callerInfo in allCallerInfo.CallerInfo)
            {
                nextid = Convert.ToInt16(callerInfo.callerID);
            }


                return nextid + 1;
        }
    }
}
