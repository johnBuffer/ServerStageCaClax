using IoTServer;
using System;

namespace ServerTest
{
    class IoTService : IIoTService
    {
        public ServiceReponse<string> GetEpochTime()
        {
            return new ServiceReponse<string> { Result = true, Name = "GetEpochTime", Payload = DateTime.Now.ToString() };
        }
    }
}
