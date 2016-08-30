using System;
using System.IO;
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


        public static string InsertLine(int unitid, IoTService service)
        {
            var dbAccess = new DatabaseAccess(service.GetIp(), service.GetPort(), service.GetName(), service.GetUser(), service.GetPassword());

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
                        flag = DeleteLine(id, service);
                    } while (!flag);
                    return text;
                }
                else if (!request.HasRows)
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


        public int ReadProgram()
        {
            var ci = new System.Globalization.CultureInfo("en-us");
            string[] result;
            char[] delimiter = { '>' };
            char[] delimiter2 = { ' ' };
            int i = 0;

            string path = @"C:\Users\dev1\Documents\Thermostat_programs\today_program";
            result = File.ReadAllLines(path);

            foreach (string elt in result)
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
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
