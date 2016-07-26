using IoTServer;
using MySql.Data.MySqlClient;
using System;

namespace ServerTest
{
    class IoTService : IIoTService
    {
        public ServiceReponse<string> GetEpochTime()
        {
            return new ServiceReponse<string> { Result = true, Name = "GetEpochTime", Payload = DateTime.Now.ToString() };
        }

        public void Ping(int unitid, string date)
        {
            var dbAccess = new DatabaseAccess("127.0.0.1", "3306", "test", "Jean", "Stageensuede1");
            dbAccess.insertValue("pings", "Unit_ID", "Timestamp", unitid.ToString(), DateTime.Now.ToString());
        }
    }
}
