using IoTServer;
using System;
using System.Diagnostics;

namespace ServerTest
{
    class IoTService : IIoTService
    {
        private string _ip = "127.0.0.1";
        private string _port = "3306";
        private string _dataBaseName = "test";
        private string _user = "Jean";
        private string _password = "Stageensuede1";

        public ServiceReponse<string> GetEpochTime()
        {
            return new ServiceReponse<string> { Result = true, Name = "GetEpochTime", Payload = DateTime.Now.ToString() };
        }

        // add a ping in the pings table
        public ServiceReponse<bool> AddPing(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.InsertValue("pings", "Unit_ID", "Timestamp", unitid.ToString(), DateTime.Now.ToString());
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = false };
            }

            // Check for pending actions
            var pendingActions = dbAccess.Request("select ID, ActionName from actions where Unit_ID='" + unitid.ToString() + "' and Status='PENDING'");

            if (pendingActions.Read())
            {
                var action = pendingActions["ActionName"].ToString();
                var actionID = pendingActions["ID"].ToString();
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = true, Action = action, ActionID = actionID };
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = true };
        }

        // add a pending action in the actions table
        public ServiceReponse<bool> AddAction(int unitid, string action)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

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

        // Update an action status to 'DONE'
        public ServiceReponse<bool> ActionDone(string actionId)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);
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
                    dbAccess.ExecuteRequest("update features f inner join devices_features df on df.Feature_ID = f.ID and df.Unit_ID = '" + unitid.ToString() + "' and f.Name = 'Relay' set State = 'ON'");
                }
                catch (ConnectionErrorException e)
                {
                    Console.WriteLine("Request error : {0}", e.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
                }
            }
            else if (object.Equals(actionName, "RelayOFF"))
            {
                try
                {
                    dbAccess.ExecuteRequest("update features f inner join devices_features df on df.Feature_ID = f.ID and df.Unit_ID = '" + unitid.ToString() + "' and f.Name = 'Relay' set State = 'OFF'");
                }
                catch (ConnectionErrorException e)
                {
                    Console.WriteLine("Request error : {0}", e.Message);
                    return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
                }
            }

            return new ServiceReponse<bool> { Result = true, Name = "ActionDone", Payload = true };
        }

        // Add a measurement
        public ServiceReponse<bool> AddMeasure(int unitid, int temp, int humidity)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.InsertValue("devices_state", "Feature_ID", "Device_ID", "Value", "Status", "Timestamp", "1", unitid.ToString(), temp.ToString(), "OK", DateTime.Now.ToString());
                dbAccess.InsertValue("devices_state", "Feature_ID", "Device_ID", "Value", "Status", "Timestamp", "3", unitid.ToString(), humidity.ToString(), "OK", DateTime.Now.ToString());
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddTemperature", Payload = false };
            }

            var tempGoal = dbAccess.Request("select GoalValue from devices_features, features where devices_features.Feature_ID = features.ID and devices_features.Unit_ID = '" + unitid.ToString() + "'");

            if (tempGoal.Read())
            {
                var goalValue = tempGoal["GoalValue"].ToString();
                int intValue = Int32.Parse(goalValue);

                if (intValue > 10)
                {
                    if (temp < intValue)
                        AddAction(unitid, "RelayON");
                    else
                        AddAction(unitid, "RelayOFF");
                }
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddTemperature", Payload = true };
        }

        public ServiceReponse<bool> SetTargetTemperature(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.ExecuteRequest("update features f inner join devices_features df on df.Feature_ID = f.ID and df.Unit_ID = '" + unitid.ToString() + "' and f.Name = 'Temperature sensor' set GoalValue = '" + value.ToString() + "'");
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Request error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = true };
        }

        public ServiceReponse<string> GetRelayStatus(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            var relayStateRequest = dbAccess.Request("select State from devices_features, features where devices_features.Feature_ID = features.ID and devices_features.Unit_ID = '" + unitid.ToString() + "' and features.Name = 'Relay'");

            if (relayStateRequest.Read())
            {
                var state = relayStateRequest["State"].ToString();
                return new ServiceReponse<string> { Result = true, Name = "SetTargetTemperature", Payload = state };
            }

            return new ServiceReponse<string> { Result = true, Name = "SetTargetTemperature", Payload = "error" };
        }

        public ServiceReponse<MeasurePoint> GetLastTemperature(int unitid)
        {

            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            var tempStateRequest = dbAccess.Request("SELECT Value, Timestamp FROM `devices_state` where Device_ID = '" + unitid.ToString() + "' and Feature_ID = '1'  ORDER BY `Timestamp` DESC");

            if (tempStateRequest.Read())
            {
                var temp = tempStateRequest["Value"].ToString();
                var date = tempStateRequest["Timestamp"].ToString();
                return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastTemperature", Payload = new MeasurePoint { Value = Int32.Parse(temp), Date = date } };
            }

            MeasurePoint point = new MeasurePoint { Value = 0, Date = DateTime.Now.ToString() };

            return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastTemperature", Payload = point };
        }

        public ServiceReponse<string> GetLastPing(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            var pingRequest = dbAccess.Request("SELECT Timestamp FROM `pings` where Unit_ID = '" + unitid.ToString() + "' ORDER BY `Timestamp` DESC");

            if (pingRequest.Read())
            {
                var date = pingRequest["Timestamp"].ToString();
                return new ServiceReponse<string> { Result = true, Name = "GetLastTemperature", Payload = date };
            }

            return new ServiceReponse<string> { Result = true, Name = "GetLastTemperature", Payload = "2000-01-01 00:00:00" };
        }

        public ServiceReponse<int> GetTargetTemperature(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);
            var tempGoal = dbAccess.Request("select GoalValue from devices_features, features where devices_features.Feature_ID = features.ID and devices_features.Unit_ID = '" + unitid.ToString() + "'");

            if (tempGoal.Read())
            {
                var goalValue = tempGoal["GoalValue"].ToString();
                int intValue = Int32.Parse(goalValue);

                return new ServiceReponse<int> { Result = false, Name = "GetTargetTemperature", Payload = intValue };
            }

            return new ServiceReponse<int> { Result = false, Name = "GetTargetTemperature", Payload = 0 };
        }

        public ServiceReponse<bool> AddHumidity(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.InsertValue("devices_state", "Feature_ID", "Device_ID", "Value", "Status", "Timestamp", "3", unitid.ToString(), value.ToString(), "OK", DateTime.Now.ToString());
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddTemperature", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddTemperature", Payload = true };
        }

        public ServiceReponse<MeasurePoint> GetLastHumidity(int unitid)
        {

            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            var tempStateRequest = dbAccess.Request("SELECT Value, Timestamp FROM `devices_state` where Device_ID = '" + unitid.ToString() + "' and Feature_ID = '3' ORDER BY `Timestamp` DESC");

            if (tempStateRequest.Read())
            {
                var temp = tempStateRequest["Value"].ToString();
                var date = tempStateRequest["Timestamp"].ToString();
                return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastHumidity", Payload = new MeasurePoint { Value = Int32.Parse(temp), Date = date } };
            }

            MeasurePoint point = new MeasurePoint { Value = 0, Date = DateTime.Now.ToString() };

            return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastHumidity", Payload = point };
        }
    }
}
