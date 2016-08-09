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

            Console.WriteLine("Done. Press Enter to close");
            Console.ReadLine();
        }
	}
}
