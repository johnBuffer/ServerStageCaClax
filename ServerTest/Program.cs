using System;

namespace IoTServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Starting service...");
			MyService service = new MyService ();
            service.Init();

            /*string _ip = "127.0.0.1";
            string _port = "3306";
            string _dataBaseName = "test";
            string _user = "Jean";
            string _password = "Stageensuede1";

            var dbAccess = new ServerTest.DatabaseAccess(_ip, _port, _dataBaseName, _user, _password);

            try
            {
                dbAccess.Request("");
            }
            catch (ServerTest.SqlErrorException e)
            {
                Console.WriteLine("SQL error : {0}", e.Message);
            }*/

            Console.WriteLine("Done. Press Enter to close");
            Console.ReadLine();
        }
	}
}
