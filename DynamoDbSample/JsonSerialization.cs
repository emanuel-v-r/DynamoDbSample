using System.Text.Json;

namespace DynamoDbSample
{
	public static class JsonSerialization
	{
		public static readonly JsonSerializerOptions Settings = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true
		};
	}
}