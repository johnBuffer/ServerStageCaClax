using IoTServer;
using MySql.Data.MySqlClient;
using System;


namespace ServerTest
{
    class DatabaseGetters
    {
        public static ServiceReponse<string> GetThermostatState(DatabaseAccess dbAccess, int unitid)
        {
            MySqlDataReader tempStateRequest;
            string payload = "Error";

            try
            {
                tempStateRequest = dbAccess.Request("select State from features where Unit_ID = '" + unitid.ToString() + "' and Name = 'Thermostat'");
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Error getting thermostat state : {0}", se.Message);
                return new ServiceReponse<string> { Result = true, Name = "GetThermostatState", Payload = "Error" };
            }

            if (tempStateRequest.Read())
            {
                string state = tempStateRequest["State"].ToString();

                if (state == "ON") { payload = "ON"; }
                else { payload = "OFF"; }
            }

            return new ServiceReponse<string> { Result = true, Name = "GetRelayStatus", Payload = payload };
        }

        public static ServiceReponse<MeasurePoint> GetLastTemperature(DatabaseAccess dbAccess, int unitid)
        {
            MeasurePoint point = new MeasurePoint { Value = 0, Date = DateTime.Now.ToString() };
            MySqlDataReader tempStateRequest;

            try
            {
                tempStateRequest = dbAccess.Request("SELECT Value, Timestamp FROM `measurements` where Unit_ID = '" + unitid.ToString() + "' and Feature_ID = '1'  ORDER BY `Timestamp` DESC"); //GetLastTemperature
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return new ServiceReponse<MeasurePoint> { Result = false, Name = "GetLastTemperature", Payload = point };
            }

            if (tempStateRequest.Read())
            {
                var temp = tempStateRequest["Value"].ToString();
                var date = tempStateRequest["Timestamp"].ToString();
                point.Value = Int32.Parse(temp);
                point.Date = date;
            }

            return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastTemperature", Payload = point };
        }

        public static ServiceReponse<int> GetTargetTemperature(DatabaseAccess dbAccess, int unitid)
        {
            MySqlDataReader tempTarget;

            try
            {
                tempTarget = dbAccess.Request("select GoalValue from features where Unit_ID = '" + unitid.ToString() + "' and Name = 'Thermostat'"); //GetTargetTemperature
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Error getting target temperature : {0}", se.Message);
                return new ServiceReponse<int> { Result = false, Name = "GetTargetTemperature", Payload = 0 };
            }

            if (tempTarget.Read())
            {
                var goalValue = tempTarget["GoalValue"].ToString();
                int intValue = Int32.Parse(goalValue);
                return new ServiceReponse<int> { Result = true, Name = "GetTargetTemperature", Payload = intValue };
            }

            return new ServiceReponse<int> { Result = false, Name = "GetTargetTemperature", Payload = 0 };
        }

        public static ServiceReponse<string> GetRelayStatus(DatabaseAccess dbAccess, int unitid)
        {
            MySqlDataReader relayStateRequest;
            try
            {
                relayStateRequest = dbAccess.Request("select State from features where Unit_ID = '" + unitid.ToString() + "' and Name = 'Relay'"); //GetRelayStatus
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Error while getting relay status : {0}", e.Message);
                return new ServiceReponse<string> { Result = true, Name = "GetRelayStatus", Payload = "error" };
            }

            if (relayStateRequest.Read())
            {
                var state = relayStateRequest["State"].ToString();
                return new ServiceReponse<string> { Result = true, Name = "GetRelayStatus", Payload = state };
            }

            return new ServiceReponse<string> { Result = true, Name = "GetRelayStatus", Payload = "error" };
        }
    }
}
