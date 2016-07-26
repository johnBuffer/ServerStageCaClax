using MySql.Data.MySqlClient;
using System;

namespace ServerTest
{
    class ConnectionErrorException : System.Exception
    {
        public ConnectionErrorException(string message) : base(message) {}
    }

    class InsertionErrorException : System.Exception
    {
        public InsertionErrorException(string message) : base(message) { }
    }

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
            _ip   = ip;
            _port = port;
            _user = user;
            _dataBaseName = dataBaseName;
            _password     = password;

            _connexion = new MySqlConnection("Server=" + _ip + ";Port=" + _port + ";Database=" + _dataBaseName + ";Uid=" + _user + ";Pwd=" + _password + ";");
        }

        public void Connect()
        {
            try
            {
                _connexion.Open();
            }
            catch (MySqlException e)
            {
                throw (new ConnectionErrorException(e.Message));
            }
        }

        public void InsertValue(string tableName, params string[] values)
        {
            var sql = "insert into "+tableName+" (";
            sql += Utils.ArrayToString(values, 0, values.Length / 2) + ") values (";
            sql += Utils.ArrayToString(values, values.Length / 2, values.Length, true) + ")";

            Console.WriteLine(sql);

            var cmd = new MySqlCommand(sql, _connexion);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                throw (new InsertionErrorException(e.Message));
            }
        }
    }
}
