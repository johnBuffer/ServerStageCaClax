using MySql.Data.MySqlClient;
using System;

namespace ServerTest
{
    class DatabaseAccess
    {
        private string _user;
        private string _password;
        private string _ip;
        private string _port;
        private string _dataBaseName;

        private MySqlConnection _connexion;

        public DatabaseAccess(string ip, string port, string dataBaseName, string user, string password)
        {
            _ip = ip;
            _port = port;
            _dataBaseName = dataBaseName;
            _user = user;
            _password = password;

            _connexion = new MySqlConnection("Server="+_ip+";Port="+_port+";Database="+_dataBaseName+";Uid="+_user+";Pwd="+_password+";");
            _connexion.Open();
        }

        public void InsertValue(string tableName, params string[] values)
        {
            var sql = "insert into "+tableName+" (";
            sql += Utils.ArrayToString(values, 0, values.Length / 2);
            sql += ") values (";
            sql += Utils.ArrayToString(values, values.Length / 2, values.Length, true);
            sql += ")";

            Console.WriteLine(sql);

            var cmd = new MySqlCommand(sql, _connexion);
            cmd.ExecuteNonQuery();
        }
    }
}
