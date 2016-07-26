using ServerTest;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace IoTServer
{
	public class MyService
	{
		public void Init()
		{
			var httpBaseAddress = new Uri("http://192.168.85.1:65201");

			var sc = new WebServiceHost(typeof(IoTService), httpBaseAddress);
		
			sc.AddServiceEndpoint (typeof(IIoTService), new WebHttpBinding (), "");            

			//Open
			sc.Open();
			Console.WriteLine("Service is live now at : {0}", httpBaseAddress);
			Console.ReadLine ();
			sc.Close ();
		}
	}
}

