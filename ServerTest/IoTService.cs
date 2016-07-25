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
            var _connexion = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=test;Uid=Jean;Pwd=Stageensuede1;");
            _connexion.Open();

            var sql = "insert into pings (Unit_id, Timestamp) values ("+unitid +", '" + DateTime.Parse(date) + "')";

            var cmd = new MySqlCommand(sql, _connexion);
            cmd.ExecuteNonQuery();
        }
    }
}
