using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Models
{
 
        public class ApiEnums
        {
            /// <summary>
            /// Enumerator to ascertain validity
            /// left open to expansion
            /// </summary>
            public enum CallerStatus
            {
                /// <summary>
                /// Used to ascertain which keys are required for an operation
                /// </summary>

                requireKemValid = 0x01,
                requireSharedValid = 0x02,
                //requireSomeFutureValue1Valid = 0x04, 
                //requireSomeFutureValue2Valid = 0x08, 
                // etc
                requireAllValid = 0xFF,

            }
        }


    }

