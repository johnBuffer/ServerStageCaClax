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
	}
}
