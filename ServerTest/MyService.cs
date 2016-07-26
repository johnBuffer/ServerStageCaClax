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
			var httpBaseAddress = new Uri("http://192.168.85.207:65201");

			var sc = new WebServiceHost(typeof(IoTService), httpBaseAddress);
		
			sc.AddServiceEndpoint (typeof(IIoTService), new WebHttpBinding (), "");            

			//Open
            try
            {
                sc.Open();
                Console.WriteLine("Service is live now at : {0}", httpBaseAddress);
                Console.ReadLine();
            }
			catch (CommunicationException e)
            {
                Console.WriteLine("An exception occured : {0}", e.Message);
                sc.Abort();
            }
            finally
            {
                sc.Close();
            }
		}
	}
}

