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
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = true, Action = action, ActionID = actionID};
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
            return new ServiceReponse<bool> { Result = true, Name = "ActionDone", Payload = true };
        }

        // Add a measurement
        public ServiceReponse<bool> AddTemperature(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.InsertValue("devices_state", "Feature_ID", "Device_ID", "Value", "Status", "Timestamp", "1", unitid.ToString(), value.ToString(), "OK", DateTime.Now.ToString());
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

                if (value < intValue)
                    AddAction(unitid, "RelayON");
                else
                    AddAction(unitid, "RelayOFF");
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddTemperature", Payload = true };
        }

        public ServiceReponse<bool> SetTargetTemperature(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.ExecuteRequest("update features f inner join devices_features df on df.Feature_ID = f.ID and df.Unit_ID = '"+ unitid.ToString() + "' set GoalValue = '" + value.ToString() + "'");
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Request error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "SetTargetTemperature", Payload = true };
        }
    }
}
