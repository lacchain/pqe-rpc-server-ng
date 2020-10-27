using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Models
{

    public class CqcKeyEncapsulation
    {
        public byte[] SharedSecret;
        public byte[] CipherText;

        public CqcKeyEncapsulation() { }
    }
}
