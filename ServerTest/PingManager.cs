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
        public static ServiceReponse<bool> HandlePing(IoTService iotService, DatabaseAccess dbAccess, int unitid)
        {
            try
            {
                dbAccess.InsertValue("pings", "Unit_ID", "Timestamp", unitid.ToString(), DateTime.Now.ToString());
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = false };
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Connection error : {0}", se.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = false };
            }

            // Check for pending actions
            var pendingActions = dbAccess.Request("select ID, ActionName from actions where Unit_ID='" + unitid.ToString() + "' and Status='PENDING'");

            if (pendingActions.Read())
            {
                var action = pendingActions["ActionName"].ToString();
                var actionID = pendingActions["ID"].ToString();
                pendingActions.Close();
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = true, Action = action, ActionID = actionID };
            }

            pendingActions.Close();

            //Check for changes in the programm
            /*int temperature = ReadProgram();
            if (temperature != -1)
            {
                SetTargetTemperature(unitid, temperature);
            }*/

            // Check for thermostat actions
            if (object.Equals(DatabaseGetters.GetThermostatState(dbAccess, unitid).Payload, "ON"))
            {
                var currentTemp = DatabaseGetters.GetLastTemperature(dbAccess, unitid).Payload.Value;
                var targetTemp = DatabaseGetters.GetTargetTemperature(dbAccess, unitid).Payload;
                var relayState = DatabaseGetters.GetRelayStatus(dbAccess, unitid).Payload;

                if (currentTemp < targetTemp)
                {
                    if (!object.Equals(relayState, "ON"))
                    {
                        iotService.AddAction(unitid, "RelayON");
                    }
                }
                else
                {
                    if (!object.Equals(relayState, "OFF"))
                    {
                        iotService.AddAction(unitid, "RelayOFF");
                    }
                }
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = true };
        }
    }
}
