using IBCQC_NetCore.Models;
using MailClient;
using System;
using System.Collections.Generic;

namespace IBCQC_NetCore.Functions
{
    internal  class SplitKeyHandlerFunction
    {
        internal static ReturnKeyFormat SendKeyParts(int keyParts,byte[] secret_key)
        {
            // Split the binary data into N equal sized blocks (where N = keyParts).
            // If the data does not split exactly, then the final block is adjusted to be a little larger than the others.

            // NOTE: Debug only set in the appsettings.json - This will return the entire key in one segment

            if (Convert.ToBoolean(Startup.StaticConfig["Config:singleSetupClientKeyPartOnly"]))
            {
                keyParts = 1;
            }


            ReturnKeyFormat formattedSegment;

            int normalSegmentSize = secret_key.Length / keyParts;
            int bytesRemaining = secret_key.Length % keyParts;
            Byte[] bindata1 = new Byte[normalSegmentSize + bytesRemaining];

            int currentSegment = 1;
            int startpos = 0;

            //infinite look with break condition
            for (; ; )
            {
                int len = normalSegmentSize;

                if (currentSegment == keyParts) // If this is the final segment
                    len += bytesRemaining;      // Add the remaining bytes, which might be 0, in which case, no harm done.

                Buffer.BlockCopy(secret_key, startpos, bindata1, 0, len);         // Extract a section of the binary data
                string hexdata1 = ByteToHexBitFiddle(bindata1, len);                      // Convert the binary data to a hex string
                formattedSegment = CreateSegmentFile(hexdata1, keyParts, currentSegment); // Bundle into a json format

                // And send it
                // TODO: Send each segment string to client

                // Prepare to process the segment
                startpos += len;
                if (startpos >= secret_key.Length)
                    break;
                currentSegment++;
            }
            return formattedSegment;
        }
        //used for new channel config
  internal static ReturnKeyFormat SendKeyParts(short keyParts, byte[] secret_key, List<Channel> channels)
        {
            if (Convert.ToBoolean(Startup.StaticConfig["Config:singleSetupClientKeyPartOnly"]))
            {
                keyParts = 1;
            }


            ReturnKeyFormat formattedSegment;

            int normalSegmentSize = secret_key.Length / keyParts;
            int bytesRemaining = secret_key.Length % keyParts;
            Byte[] bindata1 = new Byte[normalSegmentSize + bytesRemaining];

            int currentSegment = 1;
            int startpos = 0;

            //infinite look with break condition
            for (; ; )
            {
                int len = normalSegmentSize;

                if (currentSegment == keyParts) // If this is the final segment
                    len += bytesRemaining;      // Add the remaining bytes, which might be 0, in which case, no harm done.

                Buffer.BlockCopy(secret_key, startpos, bindata1, 0, len);         // Extract a section of the binary data
                string hexdata1 = ByteToHexBitFiddle(bindata1, len);                      // Convert the binary data to a hex string
                formattedSegment = CreateSegmentFile(hexdata1, keyParts, currentSegment); // Bundle into a json format

                // And send it
                // TODO: Send each segment string to client

                //as we have the channels lets see what we have

                Channel sendHere = channels[currentSegment - 1];

                if (sendHere.type.ToLower() == "email")
                {

                    ExternalMail sendMail = new ExternalMail();
                    MailInformation newMail = new MailInformation();

                    newMail.fromName = Startup.StaticConfig["Mail:FromName"];
                    newMail.fromEmail = Startup.StaticConfig["Mail:FromEmail"];
                    //newMail.toName = config["Mail:ToName"];
                    //newMail.toEmail = config["Mail:ToEmail"];
                    //newMail.body = config["Mail:MmailBody"];
                    newMail.subject = Startup.StaticConfig["Mail:MailSubject"];

                    newMail.attachment = ""; //add an attachment here for testing

                    sendMail.SendThisMail(newMail);


                }



                // Prepare to process the segment
                startpos += len;
                if (startpos >= secret_key.Length)
                    break;
                currentSegment++;
            }
            return formattedSegment;
        }

        static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }       

        static string ByteToHexBitFiddle(byte[] bytes, int len)
        {
            char[] c = new char[len * 2];
            int b;
            for (int i = 0; i < len; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        static ReturnKeyFormat CreateSegmentFile(string data, int keyParts, int segmentNo)
        {
            ReturnKeyFormat segment = new ReturnKeyFormat();

            segment.expiryDate = DateTime.Now.ToUniversalTime().ToString();
            segment.checkSum = data.Length.ToString();
            segment.hexData = data;
            segment.requiredSegments = keyParts.ToString();
            segment.segmentNumber = segmentNo.ToString();

            // Need to send this out somehow for now return it
            return segment;
        }

      
    }


    public class ReturnKeyFormat
    {
        public string requiredSegments { get; set; }
        public string segmentNumber { get; set; }
        public string hexData { get; set; }
        public string expiryDate { get; set; }
        public string checkSum { get; set; }
    }

}


