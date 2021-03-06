﻿using IoTServer;
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

        public static ServiceReponse<bool> AddMeasure(IoTService iotService, DatabaseAccess dbAccess, int unitid, int temp, int humidity)
        {
            try
            {
                dbAccess.InsertValue("measurements", "Feature_ID", "Unit_ID", "Type", "Value", "Status", "Timestamp", "1", unitid.ToString(), "Temperature", temp.ToString(), "OK", DateTime.Now.ToString()); //AddMeasure temperature
                dbAccess.InsertValue("measurements", "Feature_ID", "Unit_ID", "Type", "Value", "Status", "Timestamp", "1", unitid.ToString(), "Humidity", humidity.ToString(), "OK", DateTime.Now.ToString()); //AddMeasure humidity
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

            if (object.Equals(DatabaseGetters.GetThermostatState(dbAccess, unitid).Payload, "ON"))
            {
                var targetTemp = DatabaseGetters.GetTargetTemperature(dbAccess, unitid).Payload;
                var relayState = DatabaseGetters.GetRelayStatus(dbAccess, unitid).Payload;

                if (temp < targetTemp)
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

            return new ServiceReponse<bool> { Result = true, Name = "AddMeasure", Payload = true };
        }

        public static ServiceReponse<bool> NotifyChange(DatabaseAccess dbAccess, int unitid, int value)
        {
            try
            {
                dbAccess.InsertValue("changes", "Unit_ID", "Time", "Temperature", "Status", unitid.ToString(), DateTime.Now.ToString(), value.ToString(), "TODO");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ServiceReponse<bool> { Result = false, Name = "NotifyChange", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "NotifyChange", Payload = true };
        }
    }
}
