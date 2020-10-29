using IBCQC_NetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IBCQC_NetCore.Models.ApiEnums;

namespace IBCQC_NetCore.Functions
{
    public class AESKeyValidationFunction
    {



        internal static void verifyAES(CallerInfo callerInfo, string requestType)
        {


            if (CallerValidateFunction.callerValidate(callerInfo, CallerStatus.requireKemValid))
            {
                // Check if this is the request or response
                if (requestType.ToLower() == "response")
                {




                }
                else if (requestType.ToLower() == "request")
                {

                }
                else
                {



                    // return StatusCode(400, "Invalid Request");
                }
            }



        }





}
}
