using IBCQC_NetCore.Models;
using System;
using System.Globalization;
using static IBCQC_NetCore.Models.ApiEnums;

namespace IBCQC_NetCore.Functions
{
    public class CallerValidateFunction

    {
        internal static bool kemKeyPairNeedsChanging;
        internal static bool sharedSecretNeedsChanging;

  

        ///<Summary>
        /// Validate a users session and keys
        ///</Summary>
        public static bool callerValidate(CallerInfo caller, CallerStatus status)
        {
        

            kemKeyPairNeedsChanging = false;
            sharedSecretNeedsChanging = false;

            // First, some basic checks
            if (String.IsNullOrEmpty(caller.clientCertSerialNumber))
            {
                return false;
            }
            if (caller.kemAlgorithm == "0")
            {
                return false;
            }

            

            // Use caller status to see what to validate each call may need only one of private or shared key or both

            switch (status)
            {
                case CallerStatus.requireAllValid:
                    {
                        if (!ValidateKemPrivateKey(caller))
                        {
                            kemKeyPairNeedsChanging = true;
                        }
                        if (!ValidateSharedSecret(caller))
                        {
                            sharedSecretNeedsChanging = true;
                        }
                        if (kemKeyPairNeedsChanging || sharedSecretNeedsChanging)
                            return false;
                        break;
                    }
                case CallerStatus.requireKemValid:
                    if (!ValidateKemPrivateKey(caller))
                    {
                        kemKeyPairNeedsChanging = true;
                        return false;
                    }
                    break;

                case CallerStatus.requireSharedValid:
                    if (!ValidateSharedSecret(caller))
                    {
                        sharedSecretNeedsChanging = true;
                        return false;
                    }
                    break;

                default:
                    return false;
            }
            return true;
        }


        internal static bool mustIssueKemPrivateKeyExpiryWarning(CallerInfo caller)
        {
            // Check if the key that we hold has less than 7 days remaining.
            // So with the default value of 1 year, 7 days is approx 2%.
            DateTime time1 = DateTime.Now.AddDays(7);
            string pattern = "dd-MM-yyyy hh:mm:ss";
            DateTime dt;
            DateTime.TryParseExact(caller.keyExpiryDate, pattern, null,
                                   DateTimeStyles.None, out dt);

            if (System.DateTime.Compare(time1, dt) > 0)
            {
                return true;
            }
            return false;
        }


        internal static bool mustIssueSharedSecretExpiryWarning(CallerInfo caller)
        {
            // Check if the key that we hold has less than 1 tenth of its life left.
            // So with the default value of 7200 secs (2hours), 10% is 12 mins.

         
            string pattern =  "dd-MM-yyyy hh:mm:ss";
            DateTime dt;
            DateTime.TryParseExact(caller.keyExpiryDate, pattern, null,
                                   DateTimeStyles.None, out dt);

            DateTime time1 = DateTime.Now.AddSeconds(Convert.ToInt32(caller.sharedSecretExpiryDurationInSecs) / 10);


            if (System.DateTime.Compare(time1, dt) < 0)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Check that the callers key is valid. We hold the public key. They hold the private key.
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private static bool ValidateKemPrivateKey(CallerInfo caller)
        {
            // Check if the KemPrivateKey has expired

            string pattern = "dd-MM-yyyy hh:mm:ss";
            DateTime dt;
            DateTime.TryParseExact(caller.keyExpiryDate,pattern, null,
                                   DateTimeStyles.None, out dt);
            //compare if now is earlier than the set time we OK

            if (System.DateTime.Compare(DateTime.Now, dt) < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validate the shared secret
        /// /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private static bool ValidateSharedSecret(CallerInfo caller)
        {
            // Check if the shared secret has expired

            string pattern = "dd-MM-yyyy hh:mm:ss";
            DateTime dt;
            DateTime.TryParseExact(caller.keyExpiryDate, pattern, null,
                                   DateTimeStyles.None, out dt);

            if (System.DateTime.Compare(DateTime.Now, dt) < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
