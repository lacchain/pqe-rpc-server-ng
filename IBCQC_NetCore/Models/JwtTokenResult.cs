using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Models
{
    public class JwtTokenResult
    {
            public string Token { get; set; }
            public DateTime NotBefore { get; set; }
            public DateTime NotAfter { get; set; }
        }


    }
