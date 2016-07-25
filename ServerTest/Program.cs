using MySql.Data.MySqlClient;
using System;

namespace IoTServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			Console.WriteLine ("Starting service...");
			MyService service = new MyService ();
			service.Init ();
			Console.WriteLine ("Done. Press Enter to close");
			Console.ReadLine ();

            var _connexion = new MySqlConnection("Server=192.168.1.143;Port=3306;Database=myboat;Uid=iotclient;Pwd=gizmo;");
        }
	}
}
