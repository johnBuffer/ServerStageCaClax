using IoTServer;
using System;

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

        public ServiceReponse<bool> AddPing(int unitid, string date)
        {
            var dbAccess = new DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.Connect();
            }
            catch (ConnectionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = false };
            }

            try
            {
                dbAccess.InsertValue("pings", "Unit_ID", "Timestamp", unitid.ToString(), DateTime.Now.ToString());
            }
            catch (InsertionErrorException e)
            {
                Console.WriteLine("Connection error : {0}", e.Message);
                return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = false };
            }

            return new ServiceReponse<bool> { Result = true, Name = "AddPing", Payload = true };
        }
    }
}
