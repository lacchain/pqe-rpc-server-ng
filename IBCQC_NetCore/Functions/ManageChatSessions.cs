using IBCQC_NetCore.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Functions
{
    public class ManageChatSessions
    {
        internal static ChatSessions readNodes(string filename)
        {
            var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);

            if (!System.IO.File.Exists(filePath))
            {
                //return null;
                Console.WriteLine("ERROR: get chat session failed. File not found: " + filePath);
                throw new FileNotFoundException("ERROR: get chat sessions failed. File not found: " + filePath);
            }
            string jsonString = System.IO.File.ReadAllText(filePath);

            ChatSessions allChatSessions = JsonSerializer.Deserialize<ChatSessions>(jsonString);
            if (allChatSessions.ChatSession == null)
            {
                //return null;
                Console.WriteLine("ERROR: chart sessions store failed to parse: " + filePath);
                throw new FormatException("ERROR: chart sessions store failed to parse: " + filePath);
            }
            return allChatSessions;
        }

        internal static string  GetChatSession(string initiatorSerialNumber, string participatingSerialNumber,string filename)
        {
            var allChatSessions = readNodes(filename);
            foreach (var checkSession in allChatSessions.ChatSession)
            {
                if ((checkSession.callerSerialNumber.ToLower() == initiatorSerialNumber.ToLower()) && (checkSession.participantSerialNumber.ToLower() == participatingSerialNumber.ToLower()))
                {
                   
                    //ok we need to delete this session key now as for it to be here then it is already with the person who started the chat
                    
                    
                    return checkSession.sessionKey; 
                }
            }

            return "";
        }


        internal static bool RemoveExpiredChats(string filename)
        {

            var allChatSessions = readNodes(filename);

           foreach(var chat in allChatSessions.ChatSession)
            {

                DateTime time1 = DateTime.Now.AddMinutes(15);
                string pattern = "dd-MM-yyyy hh:mm:ss";
                DateTime dt;
                DateTime.TryParseExact(chat.keyExpiryDate, pattern, null,
                                       DateTimeStyles.None, out dt);

                if (DateTime.Compare(time1, dt) > 0)
                {
                    allChatSessions.ChatSession.Remove(chat);

                    var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
                    ////serialize the new updated object to a string
                    string towrite = JsonSerializer.Serialize(allChatSessions);
                    ////overwrite the file and it will not contain the removed client
                    System.IO.File.WriteAllText(filePath, towrite);


                    //while we here remove expired sessions so over 15 minutes
                    return true;
                }
              


            }



             return false;

        }


        internal static bool RemoveChat(string serialNumber, string participantSerialNo,string filename)
        {
            try
            {

               


                var allChatSessions = readNodes(filename);

                allChatSessions.ChatSession.RemoveAll(x => x.callerSerialNumber.ToLower() == serialNumber.ToLower() &&  x.participantSerialNumber.ToLower() == participantSerialNo.ToLower());
               

                var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
                ////serialize the new updated object to a string
                string towrite = JsonSerializer.Serialize(allChatSessions);
                ////overwrite the file and it will not contain the removed client
                System.IO.File.WriteAllText(filePath, towrite);


                //while we here remove expired sessions so over 15 minutes








                return true;
            }


            catch
            {
                return false;

            }


        }


    }
}
