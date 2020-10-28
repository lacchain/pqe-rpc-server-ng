using System;


namespace IBCQC_NetCore.Models
{
    public class KeyValidate
    {
        //the string in base64 that holds the validation code

        public string keyToValidate;
        //this determines if we validate a KEM pair or AES shared secret
        //values are KEM or AES
        public string typeOfKey;
        //this detyermines if this is the request to validate or response to the request
        //values are request or response
        public string requestType;

        public DateTime issuedDate;

        public DateTime expiryDate;

        public int callerID;

        public string storedData;


        //return an object that is expired
        public KeyValidate()
        {
            this.keyToValidate = "";
            this.typeOfKey = "";
            this.requestType = "";
            this.issuedDate = DateTime.Now.AddDays(-1);
            this.expiryDate = DateTime.Now.AddDays(-1);
            this.callerID = 0;
            this.storedData = "";

        }



    }
}
