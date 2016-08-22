using System;
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
    }
}
