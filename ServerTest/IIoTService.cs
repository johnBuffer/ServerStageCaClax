using ServerTest;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace IoTServer
{
	[ServiceContract]
	public interface IIoTService
	{
        /*[OperationContract, WebGet(UriTemplate = "{filename}")]
        //[WebGet(ResponseFormat = WebMessageFormat.Xml)]
        Stream Files(string filename);*/

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
        ServiceReponse<bool> AddMeasure(int unitid, int temp, int humidity);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> SetTargetTemperature(int unitid, int value);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<string> GetRelayStatus(int unitid);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<MeasurePoint> GetLastTemperature(int unitid);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<MeasurePoint> GetLastHumidity(int unitid);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<string> GetLastPing(int unitid);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<int> GetTargetTemperature(int unitid);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> SetThermostatState(int unitid, int state);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<string> GetThermostatState(int unitid);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ServiceReponse<bool> NotifyChange(int unitid, int value);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool SaveChanges(int unitid);
    }
}
