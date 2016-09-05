using System;
using System.IO;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    class Utils
    {
        public static string ArrayToString(string[] array, int firstIndex, int lastIndex, bool quotation = false)
        {
            string result = "";
            for (int i = firstIndex; i < lastIndex; i++)
            {
                if (quotation)
                    result += "'" + array[i] + "', ";
                else
                    result += array[i] + ", ";
            }
            result = result.Remove(result.Length - 2, 2);

            return result;
        }


        public static bool DeleteLine(string id, IoTService service)
        {
            try
            {
                var dbAccess = new DatabaseAccess(service.GetIp(), service.GetPort(), service.GetName(), service.GetUser(), service.GetPassword());
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


        public static int ReadProgram(DatabaseAccess dbAccess, IoTService service, int unitid)
        {
            MySqlDataReader tempRequest;

            try
            {
                tempRequest = dbAccess.Request("select * from changes where Unit_ID = " + unitid.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }

            while (tempRequest.Read())
            {
                if(DateTime.Now.CompareTo(DateTime.Parse(tempRequest["Time"].ToString())) >= 0 && tempRequest["Status"].ToString() != "DONE")
                {
                    int temp = (int) tempRequest["Temperature"];
                    int id = (int) tempRequest["ID"];
                    tempRequest.Close();

                    try
                    {
                        dbAccess = new DatabaseAccess(service.GetIp(), service.GetPort(), service.GetName(), service.GetUser(), service.GetPassword());
                        dbAccess.Update("changes", id.ToString(), "Status", "DONE");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return -1;
                    }
                    return temp;
                }
            }

            tempRequest.Close();
            return -1;
        }


        public static string LoadProgram(DatabaseAccess dbAccess, int unitid)
        {
            string program = "";
            MySqlDataReader tempRequest;

            try
            {
                tempRequest = dbAccess.Request("select * from changes where Unit_ID = " + unitid.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "Error";
            }
            
            if(tempRequest.Read())
            {
                foreach(MySqlDataReader elt in tempRequest)
                {
                    program = program + elt["Time"].ToString() + ":" + elt["Temperature"].ToString() + ":";
                }

                tempRequest.Close();
            }

            Console.WriteLine(program);
            tempRequest.Close();
            return program;
        }
    }
}
