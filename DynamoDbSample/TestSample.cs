using System;

namespace DynamoDbSample
{
	public class TestSample
	{
		internal const string TableName = nameof(TestSample);

		public Guid Id { get; init; }

		public string EmptyStr { get; init; }
	}
}