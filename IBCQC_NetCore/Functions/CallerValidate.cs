using IBCQC_NetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IBCQC_NetCore.Models.ApiEnums;

namespace IBCQC_NetCore.Functions
{
    public class CallerValidate
    {
        public bool kemKeyPairNeedsChanging;
        public bool sharedSecretNeedsChanging;

        public CallerValidate()
        {
            kemKeyPairNeedsChanging = false;
            sharedSecretNeedsChanging = false;
        }

        ///<Summary>
        /// Validate a users session and keys
        ///</Summary>
        public bool callerValidate(CallerInfo caller, CallerStatus status)
        {
            // For example...
            //   callerId	0x00000000	int
            //   clientCertName	""	string
            //   clientCertSerialNumber	null	string
            //   isInitialise	false	bool
            //   kemAlgorithm	0x00000000	int
            //   kemPrivateKey	null	byte[]
            //   kemPublicKey	null	byte[]
            //  +keyExpiryDate	{03/09/2021 15:06:20}	System.DateTime
            //   sharedSecretExpiryDurationInSecs	0x0000000000000000	long
            //  +sharedSecretExpiryTime	{03/09/2020 15:06:20}	System.DateTime
            //   sharedSecretForSession	null	byte[]

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

            // Not sure on concensus but using calculated values to see where we are

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


        public bool mustIssueKemPrivateKeyExpiryWarning(CallerInfo caller)
        {
            // Check if the key that we hold has less than 7 days remaining.
            // So with the default value of 1 year, 7 days is approx 2%.
            DateTime time1 = DateTime.Now.AddDays(7);
            DateTime time2 = Convert.ToDateTime(caller.keyExpiryDate);

            if (System.DateTime.Compare(time1, time2) < 0)
            {
                return false;
            }
            return true;
        }


        public bool mustIssueSharedSecretExpiryWarning(CallerInfo caller)
        {
            // Check if the key that we hold has less than 1 tenth of its life left.
            // So with the default value of 7200 secs (2hours), 10% is 12 mins.
            DateTime time1 = DateTime.Now.AddSeconds(Convert.ToInt16(caller.sharedSecretExpiryDurationInSecs) / 10);
            DateTime time2 = Convert.ToDateTime(caller.sharedSecretExpiryTime);

            if (System.DateTime.Compare(time1, time2) < 0)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Check that the callers key is valid. We hold the public key. They hold the private key.
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private bool ValidateKemPrivateKey(CallerInfo caller)
        {
            // Check if the KemPrivateKey has expired

            if (System.DateTime.Compare(DateTime.Now, Convert.ToDateTime(caller.keyExpiryDate)) < 0)
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
        private bool ValidateSharedSecret(CallerInfo caller)
        {
            // Check if the shared secret has expired

            if (System.DateTime.Compare(DateTime.Now, Convert.ToDateTime(caller.sharedSecretExpiryTime)) < 0)
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
