using IoTServer;
using MySql.Data.MySqlClient;
using System;

namespace ServerTest
{
    class DatabaseSetters
    {
        public static ServiceReponse<bool> AddAction(DatabaseAccess dbAccess, int unitid, string action)
        {
            try
            {
                dbAccess.InsertValue("actions", "Unit_ID", "ActionName", "Status", unitid.ToString(), action, "PENDING");
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddAction", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddAction", Payload = true };
        }

        public static ServiceReponse<string> ChangeProgram(DatabaseAccess dbAccess, int unitid, string program)
        {
            try
            {
                dbAccess.InsertValue("actions", "Unit_ID", "ActionName", "Status", unitid.ToString(), "UpdateProgram", "PENDING");
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<string> { Result = true, Name = "AddAction", Payload = null };
            }

            return new ServiceReponse<string> { Result = true, Name = "AddAction", Payload = program };
        }

        public static ServiceReponse<bool> SetTargetTemperature(DatabaseAccess dbAccess, int unitid, int value)
        {
            try
            {
                dbAccess.ExecuteRequest("update features set GoalValue = '" + value.ToString() + "' where Unit_ID = '" + unitid.ToString() + "' and Name = 'Thermostat'"); //SetTargetTemperature
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Request error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Error adding temperature : {0}", se.Message);
                return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = true };
        }

        public static ServiceReponse<bool> SetThermostatState(DatabaseAccess dbAccess, int unitid, int state)
        {
            if (state == 0)
            {
                try
                {
                    dbAccess.ExecuteRequest("update features set State = 'OFF' where Unit_ID = '" + unitid.ToString() + "' and Name = 'Thermostat'");
                }
                catch (ConnectionErrorException e)
                {
                    Console.WriteLine("Request error : {0}", e.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
                }
                catch (InvalidOperationException se)
                {
                    Console.WriteLine("Error setting relay : {0}", se.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "SetRelayStatus", Payload = false };
                }
            }
            else
            {
                try
                {
                    dbAccess.ExecuteRequest("update features set State = 'ON' where Unit_ID = '" + unitid.ToString() + "' and Name = 'Thermostat'");
                }
                catch (ConnectionErrorException e)
                {
                    Console.WriteLine("Request error : {0}", e.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
                }
                catch (InvalidOperationException se)
                {
                    Console.WriteLine("Error setting relay : {0}", se.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "SetRelayStatus", Payload = false };
                }
            }

            return new ServiceReponse<bool> { Result = true, Name = "SetThermostatState", Payload = true };
        }

        public static ServiceReponse<bool> NotifyChange(DatabaseAccess dbAccess, int unitid, int value)
        {
            try
            {
                dbAccess.InsertValue("changes", "Unit_ID", "TargetTemperature", "Timestamp", unitid.ToString(), value.ToString(), DateTime.Now.ToString());
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Request error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Error notifying change : {0}", se.Message);
                return new ServiceReponse<bool> { Result = true, Name = "NotifyChange", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "NotifyChange", Payload = true };
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
