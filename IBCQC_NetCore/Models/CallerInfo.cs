using System.Collections.Generic;

namespace IBCQC_NetCore.Models
{



    public class CallerInfo
    {
        public string callerID { get; set; }
        public string kemAlgorithm { get; set; }
        public string kemPublicKey { get; set; }
        public string kemPrivateKey { get; set; }
        public string sharedSecretExpiryDurationInSecs { get; set; }
        public string sharedSecretForSession { get; set; }
        public string sharedSecretExpiryTime { get; set; }
        public string keyExpiryDate { get; set; }
        public string clientCertSerialNumber { get; set; }
        public string clientCertName { get; set; }
        public string isInitialise { get; set; }
}

public class AllCallerInfo
{
    public List<CallerInfo> CallerInfo { get; set; }
}



    }
