using IoTServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel.Web;

namespace ServerTest
{
    class IoTService : IIoTService
    {
        private string _ip = "127.0.0.1";
        private string _port = "3306";
        private string _dataBaseName = "db_stage";
        private string _user = "Ugo";
        private string _password = "voielacte";

        public ServiceReponse<string> GetEpochTime()
        {
            return new ServiceReponse<string> { Result = true, Name = "GetEpochTime", Payload = DateTime.Now.ToString() };
        }

        // add a ping in the pings table
        public ServiceReponse<bool> AddPing(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return PingManager.HandlePing(this, dbAccess, unitid);
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

        // Add a update program action, basically the same as add action but return a string payload with the program
        public ServiceReponse<string> ChangeProgram(int unitid, string program)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

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

        // Update an action status to 'DONE'
        public ServiceReponse<bool> ActionDone(string actionId)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return ActionManager.SetActionDone(dbAccess, actionId);
        }

        // Add a measurement
        public ServiceReponse<bool> AddMeasure(int unitid, int temp, int humidity)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return PingManager.AddMeasure(this, dbAccess, unitid, temp, humidity);
        }

        public ServiceReponse<bool> SetTargetTemperature(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

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

        public ServiceReponse<string> GetRelayStatus(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseGetters.GetRelayStatus(dbAccess, unitid);
        }

        public ServiceReponse<MeasurePoint> GetLastTemperature(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseGetters.GetLastTemperature(dbAccess, unitid);
        }

        public ServiceReponse<string> GetLastPing(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                var pingRequest = dbAccess.Request("SELECT Timestamp FROM `pings` where Unit_ID = '" + unitid.ToString() + "' ORDER BY `Timestamp` DESC");

                if (pingRequest.Read())
                {
                    var date = pingRequest["Timestamp"].ToString();
                    return new ServiceReponse<string> { Result = true, Name = "GetLastTemperature", Payload = date };
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Error getting last ping : {0}", e.Message);
                return new ServiceReponse<string> { Result = true, Name = "GetLastPing", Payload = "2000-01-01 00:00:00" };
            }

            return new ServiceReponse<string> { Result = true, Name = "GetLastPing", Payload = "2000-01-01 00:00:00" };
        }

        public ServiceReponse<int> GetTargetTemperature(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseGetters.GetTargetTemperature(dbAccess, unitid);
        }

        public ServiceReponse<MeasurePoint> GetLastHumidity(int unitid)
        {
            MeasurePoint point = new MeasurePoint { Value = 0, Date = DateTime.Now.ToString() };

            try
            {
                var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

                var tempStateRequest = dbAccess.Request("SELECT Value, Timestamp FROM `measurements` where Unit_ID = '" + unitid.ToString() + "' and Feature_ID = '3' ORDER BY `Timestamp` DESC"); //GetLastHumidity

                if (tempStateRequest.Read())
                {
                    var temp = tempStateRequest["Value"].ToString();
                    var date = tempStateRequest["Timestamp"].ToString();
                    return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastHumidity", Payload = new MeasurePoint { Value = Int32.Parse(temp), Date = date } };
                }
            }
            catch (InvalidOperationException se)
            {
                Console.WriteLine("Error getting last humidity : {0}", se.Message);
                return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastHumidity", Payload = point };
            }

            return new ServiceReponse<MeasurePoint> { Result = true, Name = "GetLastHumidity", Payload = point };
        }

        public ServiceReponse<bool> SetThermostatState(int unitid, int state)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            if(state == 0)
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

        public ServiceReponse<string> GetThermostatState(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseGetters.GetThermostatState(dbAccess, unitid);
        }

        public ServiceReponse<bool> NotifyChange(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

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

        public ServiceReponse<bool> SaveChanges(int unitid)
        {
            string result = InsertLine(unitid);
            List<string> text = new List<string>();
            while(result != "Finished")
            {
                if(result != "Error")
                {
                    text.Add(result);
                    result = InsertLine(unitid);
                }
            }

            string path = @"C:\Users\dev1\Documents\Thermostat_programs\today_program";
            File.WriteAllLines(path, text);

            string oneLineProgram = "";
            foreach(string elt in text)
            {
                oneLineProgram = oneLineProgram + elt + ";";
            }

            ChangeProgram(unitid, oneLineProgram);
            return new ServiceReponse<bool> { Result = true, Name = "SaveChanges", Payload = true};
        }

        public string InsertLine(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                var request = dbAccess.Request("select ID, TargetTemperature, Timestamp from changes where Unit_ID = '" + unitid.ToString() + "' order by Timestamp asc");

                if (request.Read())
                {
                    string id = request["ID"].ToString();
                    string temperature = request["TargetTemperature"].ToString();
                    string time = request["Timestamp"].ToString();
                    string text = time + ">" + temperature + ">TODO";

                    bool flag = false;
                    do
                    {
                        flag = DeleteLine(id);
                    } while (!flag);
                    return text;
                }
                else if(!request.HasRows)
                {
                    return "Finished";
                }
                else
                {
                    return "Error";
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return "Error";
            }
        }

        public bool DeleteLine(string id)
        {
            try
            {
                var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);
                dbAccess.ExecuteRequest("delete from changes where ID = '" + id + "'");
            }
            catch (ConnectionErrorException e)
            {
                return false;
            }
            catch (InvalidOperationException se)
            {
                return false;
            }

            return true;
        }

        public int ReadProgram()
        {
            var ci = new System.Globalization.CultureInfo("en-us");
            string[] result;
            char[] delimiter = { '>' };
            char[] delimiter2 = { ' ' };
            int i = 0;

            string path = @"C:\Users\dev1\Documents\Thermostat_programs\today_program";
            result = File.ReadAllLines(path);

            foreach(string elt in result)
            {
                string[] splited_string = elt.Split(delimiter);
                //if (splited_string[0] == DateTime.Now.ToString().Split(delimiter2)[1] && splited_string[2] != "DID")
                //Console.WriteLine("Read one = "  + DateTime.Parse(splited_string[0]) + " / Real one = " + DateTime.Now + " / Comparison = " + DateTime.Parse(splited_string[0]).CompareTo(DateTime.Now));
                if ((DateTime.Parse(splited_string[0]).CompareTo(DateTime.Now) <= 0) && splited_string[2] != "DONE")
                {
                    int temperature = int.Parse(splited_string[1]);
                    string line_base = splited_string[0] + ">" + splited_string[1];
                    Console.WriteLine("Line base = " + line_base);

                    bool validation;
                    do
                    {
                        validation = ValidateChangeInProgram(path, result, line_base, i);
                    } while (!validation);

                    return temperature;
                }
                i++;
            }

            return -1;
        }

        public bool ValidateChangeInProgram(string path, string[] text, string line_base, int i)
        {
            string line = line_base + ">DONE";
            Console.WriteLine("Line to be written : " + line);
            text[i] = line;

            try
            {
                File.WriteAllLines(path, text);
            }
            catch(Exception e)
            {
                return false;
            }

            return true;
        }

        /*public Stream Files(string filename)
        {
            Console.WriteLine("request file : "+filename);

            Stream stream = null;

            try
            {
                stream = (Stream)new FileStream(filename, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //Set the correct context type for the file requested.
            int extIndex = filename.LastIndexOf(".");
            string extension = filename.Substring(extIndex, filename.Length - extIndex);
            switch (extension)
            {
                case ".html":
                case ".htm":
                    WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
                    break;
                case ".css":
                    WebOperationContext.Current.OutgoingResponse.ContentType = "text/css";
                    break;
                case ".js":
                    WebOperationContext.Current.OutgoingResponse.ContentType = "text/script";
                    break;
                case ".jpeg":
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image";
                    break;
                case ".png":
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image";
                    break;
                default:
                    Console.WriteLine("File not supported");
                    break;
            }

            return stream;
        }*/
    }
}
