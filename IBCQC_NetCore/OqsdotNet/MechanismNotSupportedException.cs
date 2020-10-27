using System;

namespace IBCQC_NetCore.OqsdotNet
{
    /// <summary>
    /// Thrown when a requested mechanism is not supported by OQS.
    /// </summary>
    public class MechanismNotSupportedException : Exception
    {
        /// <summary>
        /// The non-supported mechanism.
        /// </summary>
        public string RequestedMechanism { private set; get; }

        /// <summary>
        /// Constructs a new <see cref="MechanismNotSupportedException"/>.
        /// </summary>
        /// <param name="mechanism"></param>
        public MechanismNotSupportedException(string mechanism) : base(mechanism + " not supported")
        {
            RequestedMechanism = mechanism;
        }
    }
}
