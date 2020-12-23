using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Models
{
    // ChatSessions myDeserializedClass = JsonConvert.DeserializeObject<ChatSessions>(myJsonResponse); 
    public class ChatSession
    {
        public string callerSerialNumber { get; set; }
        public string participantSerialNumber { get; set; }
        public string sessionKey { get; set; }
        public string keyExpiryDate { get; set; }
    }

    public class ChatSessions
    {
        public List<ChatSession> ChatSession { get; set; }
    }
}
