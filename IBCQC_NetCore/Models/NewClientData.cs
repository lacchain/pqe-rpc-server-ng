using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Models
{



    public class Channel
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class NewClientData
    {
        public string clientCertName { get; set; }
        public string clientCertSerialNumber { get; set; }
        public string countryCode { get; set; }
        public List<Channel> channels { get; set; }
        public string kemAlgorithm { get; set; }
    }


}
