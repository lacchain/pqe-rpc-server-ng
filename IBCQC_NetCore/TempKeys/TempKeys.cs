using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.TempKeys
{
    // StoredKemKeys myDeserializedClass = JsonConvert.DeserializeObject<StoredKemKeys>(myJsonResponse); 
    public class KemKey
    {
        public string keyID { get; set; }
        public string keyAlgorithm { get; set; }
        public string kemPublicKey { get; set; }
        public string kemPrivateKey { get; set; }
        public string isUsed { get; set; }
    }

    public class StoredKemKeys
    {
        public List<KemKey> KemKeys { get; set; }

    }

}

