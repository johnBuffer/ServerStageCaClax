using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace IoTServer
{
	[ServiceContract]
	public interface IIoTService
	{
		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		ServiceReponse<string> GetEpochTime ();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> AddPing (int unitid, String date);
    }
}
