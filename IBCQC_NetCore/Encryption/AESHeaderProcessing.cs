using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Encryption
{
   internal static class AESHeaderProcessing
    {

        internal static byte[] AddEncryptHeader(int reqDataSize, byte[] encryptedBytes)
        {
           // 4 bytes for datasize 16 bytes for checksum 
          
           var  dsizeByte = HostToNetworkOrder(reqDataSize);
            //datetime too big a value anmd unix issues after 2038 so  put checksum in

            int dsize = dsizeByte.Length;

            var strcheckSum = MD5CheckSum.CalculateMD5Hash(encryptedBytes);
         //test this length 
            
            var checkSum = HostToNetworkOrder(strcheckSum);

           
            Array.Resize(ref dsizeByte, dsize + checkSum.Length);

            
            Array.Copy(checkSum, 0, dsizeByte, dsize,checkSum.Length);

            int newDsizeLen = dsizeByte.Length; 

            Array.Resize(ref dsizeByte, dsizeByte.Length + encryptedBytes.Length);
            Array.Copy(encryptedBytes, 0, dsizeByte,newDsizeLen, encryptedBytes.Length);

            return dsizeByte;
        }



        internal static int RemoveEncryptHeader(byte[] data)
        {

            //get the size of the data 

           int datasize  = NetworkByteToHostOrder(data.Take(4).ToArray());
           string checkSum  = NetworkToHostOrder(data.Skip(4).Take(32).ToArray());
            //checksum is 32 bytes


            //TODO verifyChecksum

            int datalen = data.Length;

            string calulatedChkSum = MD5CheckSum.CalculateMD5Hash(data.Skip(36).Take(datalen).ToArray());

            if (calulatedChkSum == checkSum)
            {

                return datasize;
            }
            
            else
            {
                return 0;
                
            }


        
        }



        /// <summary>
        /// Convert an string to network order
        /// </summary>
        /// <param name="host">string  to convert</param>
        /// <returns>string in network order</returns>
        private static byte[] HostToNetworkOrder(string checkSum)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(checkSum);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }


        /// <summary>
        /// Convert a string  to host order it is ASCII
        /// </summary>
        /// <param name="network">int32  to convert</param>
        /// <returns>Int32 in host order</returns>
        public static string NetworkToHostOrder(byte[] checkSum)
        {
           if (BitConverter.IsLittleEndian)
                Array.Reverse(checkSum);
           string chkSumValue  = Encoding.ASCII.GetString(checkSum);



            return chkSumValue;
        }



        /// <summary>
        /// Convert an int to network order
        /// </summary>
        /// <param name="host">Int32  to convert</param>
        /// <returns>Float in network order</returns>
        private static byte[] HostToNetworkOrder(int host)
        {
            byte[] bytes = BitConverter.GetBytes(host);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }





        /// <summary>
        /// Convert a int to host order
        /// </summary>
        /// <param name="network">int32  to convert</param>
        /// <returns>Int32 in host order</returns>
        public static int NetworkToHostOrder(int network)
        {
            byte[] bytes = BitConverter.GetBytes(network);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }


        /// <summary>
        /// Convert a revceived byte array to host order Int32
        /// </summary>
        /// <param name="header">bytearray recieved  to convert</param>
        /// <returns>Int32 in host order</returns>
        public static int NetworkByteToHostOrder(byte[] header)
        {
           

            if (BitConverter.IsLittleEndian)
                Array.Reverse(header);

            return BitConverter.ToInt32(header, 0);
        }



    }
}
