﻿using MySql.Data.MySqlClient;
using System;

namespace ServerTest
{
    class ConnectionErrorException : System.Exception
    {
        public ConnectionErrorException(string message) : base(message) {}
    }

    class SqlErrorException : System.Exception
    {
        public SqlErrorException(string message) : base(message) { }
    }

    class DatabaseAccess
    {
        private string _user;
        private string _password;
        private string _ip;
        private string _port;
        private string _dataBaseName;

        private MySqlConnection _connection;

        public DatabaseAccess(string ip, string port, string dataBaseName, string user, string password)
        {
            _ip   = ip;
            _port = port;
            _user = user;
            _dataBaseName = dataBaseName;
            _password     = password;

            _connection = new MySqlConnection("Server=" + _ip + ";Port=" + _port + ";Database=" + _dataBaseName + ";Uid=" + _user + ";Pwd=" + _password + ";");

            try
            {
                Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        ~DatabaseAccess()
        {
            _connection.Close();
        }

        private void Connect()
        {
            try
            {
                _connection.Open();
            }
            catch (Exception e)
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

            var cmd = new MySqlCommand(sql, _connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                throw (new SqlErrorException(e.Message));
            }
            catch (System.InvalidOperationException se)
            {
                throw (new InvalidOperationException(se.Message));
            }
        }

        public MySqlDataReader Request(string sqlRequest)
        {
            MySqlDataReader result;
            var cmd = new MySqlCommand(sqlRequest, _connection);

            try
            {
                result = cmd.ExecuteReader();
            }
            catch (MySqlException e)
            {
                throw (new SqlErrorException(e.Message));
            }
            catch (InvalidOperationException se)
            {
                throw (new InvalidOperationException(se.Message));
            }

            return result;
        }

        public void Update(string tableName, string id, string column, string value)
        {
            var sql = "update "+ tableName + " set " + column + " = '" + value + "' where ID = '" + id + "';";

            Console.WriteLine(sql);

            var cmd = new MySqlCommand(sql, _connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                throw (new SqlErrorException(e.Message));
            }
        }

        public void ExecuteRequest(String request)
        {
            var cmd = new MySqlCommand(request, _connection);
            Console.WriteLine(request);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                throw (new SqlErrorException(e.Message));
            }
        }
    }
}
