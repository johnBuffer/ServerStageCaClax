namespace IoTServer
{
	public class ServiceReponse<T>
	{
		public string Name  { get; set;}
		public bool Result  { get; set;}
		public T Payload    { get; set;}
		public string Error { get; set;}
	}
}
