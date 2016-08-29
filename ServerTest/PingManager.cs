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
            if (DatabaseGetters.GetThermostatState(dbAccess, unitid).Payload == "ON")
            {
                var currentTemp = DatabaseGetters.GetLastTemperature(dbAccess, unitid).Payload.Value;
                var targetTemp = DatabaseGetters.GetTargetTemperature(dbAccess, unitid).Payload;
                var relayState = DatabaseGetters.GetRelayStatus(dbAccess, unitid).Payload;

                if (currentTemp < targetTemp)
                {
                    if (relayState != "ON")
                    {
                        iotService.AddAction(unitid, "RelayON");
                    }
                }
                else
                {
                    if (relayState != "OFF")
                    {
                        iotService.AddAction(unitid, "RelayOFF");
                    }
                }
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = true };
        }

        public static ServiceReponse<bool> AddMeasure(IoTService iotService, DatabaseAccess dbAccess, int unitid, int temp, int humidity)
        {
            try
            {
                dbAccess.InsertValue("measurements", "Feature_ID", "Unit_ID", "Type", "Value", "Status", "Timestamp", "1", unitid.ToString(), "Temperature", temp.ToString(), "OK", DateTime.Now.ToString()); //AddMeasure temperature
                dbAccess.InsertValue("measurements", "Feature_ID", "Unit_ID", "Type", "Value", "Status", "Timestamp", "1", unitid.ToString(), "Temperature", humidity.ToString(), "OK", DateTime.Now.ToString()); //AddMeasure humidity
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddMeasure", Payload = false };
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Error adding measure : {0}", se.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddMeasure", Payload = false };
            }

            var tempGoal = dbAccess.Request("select GoalValue from features where Unit_ID = '" + unitid.ToString() + "' and Name = 'Relay'"); //AddMeasure

            if (tempGoal.Read())
            {
                var goalValue = tempGoal["GoalValue"].ToString();
                int intValue = Int32.Parse(goalValue);

                if (object.Equals(DatabaseGetters.GetThermostatState(dbAccess, unitid).Payload, "ON"))
                {
                    if (temp < intValue)
                        iotService.AddAction(unitid, "RelayON");
                    else
                        iotService.AddAction(unitid, "RelayOFF");
                }
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddMeasure", Payload = true };
        }
    }
}
