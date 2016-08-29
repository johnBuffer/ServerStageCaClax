using IoTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    class ActionManager
    {
        public static ServiceReponse<bool> SetActionDone(DatabaseAccess dbAccess, string actionId)
        {
            try
            {
                dbAccess.Update("actions", actionId, "Status", "DONE");
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "ActionDone", Payload = false };
            }

            var actionNameRequest = dbAccess.Request("select ActionName from actions where ID = '" + actionId + "'");
            string actionName = "";

            if (actionNameRequest.Read())
            {
                actionName = actionNameRequest["ActionName"].ToString();
                actionNameRequest.Close();
            }

            var unitidRequest = dbAccess.Request("select Unit_ID from actions where ID = '" + actionId + "'");
            int unitid = -1;

            if (unitidRequest.Read())
            {
                unitid = Int32.Parse(unitidRequest["Unit_ID"].ToString());
                unitidRequest.Close();
            }

            if (object.Equals(actionName, "RelayON"))
            {
                try
                {
                    dbAccess.ExecuteRequest("update features f set State = 'ON' where Unit_ID = '" + unitid.ToString() + "' and Name = 'Relay'"); //Action done set relay ON
                }
                catch (ConnectionErrorException e)
                {
                    Console.WriteLine("Request error : {0}", e.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "ActionDone", Payload = false };
                }
            }
            else if (object.Equals(actionName, "RelayOFF"))
            {
                try
                {
                    dbAccess.ExecuteRequest("update features f set State = 'OFF' where Unit_ID = '" + unitid.ToString() + "' and Name = 'Relay'"); //Action done set relay OFF
                }
                catch (ConnectionErrorException e)
                {
                    Console.WriteLine("Request error : {0}", e.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "ActionDone", Payload = false };
                }
                catch (InvalidOperationException se)
                {
                    Console.WriteLine("Error in action done : {0}", se.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "ActionDone", Payload = false };
                }
            }

            return new ServiceReponse<bool> { Result = true, Name = "ActionDone", Payload = true };
        }
    }
}
