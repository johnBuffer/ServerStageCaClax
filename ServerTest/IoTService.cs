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
        

        /******************* Getters ***************/
        public string GetIp() {
            return _ip;
        }
        public string GetPort() {
            return _port;
        }
        public string GetName() {
            return _dataBaseName;
        }
        public string GetUser() {
            return _user;
        }
        public string GetPassword() {
            return _password;
        }
        /***************** End getters *************/


        public ServiceReponse<string> GetEpochTime()
        {
            return new ServiceReponse<string> { Result = true, Name = "GetEpochTime", Payload = DateTime.Now.ToString() };
        }


        // add a ping in the pings table
        public ServiceReponse<string> AddPing(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return PingManager.HandlePing(this, dbAccess, unitid);
        }


        // add a pending action in the actions table
        public ServiceReponse<bool> AddAction(int unitid, string action)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseSetters.AddAction(dbAccess, unitid, action);
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

            return DatabaseSetters.AddMeasure(this, dbAccess, unitid, temp, humidity);
        }


        public ServiceReponse<bool> SetTargetTemperature(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseSetters.SetTargetTemperature(dbAccess, unitid, value);
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

            return DatabaseGetters.GetlastPing(dbAccess, unitid);
        }


        public ServiceReponse<int> GetTargetTemperature(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseGetters.GetTargetTemperature(dbAccess, unitid);
        }


        public ServiceReponse<MeasurePoint> GetLastHumidity(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseGetters.GetLastHumidity(dbAccess, unitid);
        }


        public ServiceReponse<bool> SetThermostatState(int unitid, int state)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseSetters.SetThermostatState(dbAccess, unitid, state);
        }


        public ServiceReponse<string> GetThermostatState(int unitid)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseGetters.GetThermostatState(dbAccess, unitid);
        }


        public ServiceReponse<bool> NotifyChange(int unitid, int value)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            return DatabaseSetters.NotifyChange(dbAccess, unitid, value);
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
