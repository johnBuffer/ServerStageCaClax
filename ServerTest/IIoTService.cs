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
        ServiceReponse<bool> AddPing (int unitid);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> AddAction(int unitid, string action);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> ActionDone(string actionId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> AddTemperature(int unitid, int value);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> SetTargetTemperature(int unitid, int value);
    }
}
