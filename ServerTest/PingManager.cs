using IoTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    class PingManager
    {
        public static ServiceReponse<string> HandlePing(IoTService iotService, DatabaseAccess dbAccess, int unitid)
        {
            try
            {
                dbAccess.InsertValue("pings", "Unit_ID", "Timestamp", unitid.ToString(), DateTime.Now.ToString());
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<string> { Result = false, Name = "AddPing", Payload = "" };
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Connection error : {0}", se.Message);
                return new ServiceReponse<string> { Result = false, Name = "AddPing", Payload = "" };
            }

            // Check for pending actions
            var pendingActions = dbAccess.Request("select ID, ActionName from actions where Unit_ID='" + unitid.ToString() + "' and Status='PENDING'");

            if (pendingActions.Read())
            {
                var action = pendingActions["ActionName"].ToString();
                string payload = "";

                if(object.Equals(action, "UpdateProgram"))
                {
                    payload = Utils.LoadProgram(dbAccess, unitid);
                }
                var actionID = pendingActions["ID"].ToString();
                pendingActions.Close();
                return new ServiceReponse<string> { Result = true, Name = "AddPing", Payload = payload, Action = action, ActionID = actionID };
            }

            pendingActions.Close();

            //Check for changes in the programm
            int temperature = Utils.ReadProgram(dbAccess, iotService, unitid);
            if (temperature != -1)
            {
                iotService.SetTargetTemperature(unitid, temperature);
            }

            // Check for thermostat actions

            return new ServiceReponse<string> { Result = true, Name = "AddPing", Payload = "" };
        }
    }
}
