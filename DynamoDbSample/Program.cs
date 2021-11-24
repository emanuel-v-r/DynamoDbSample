using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;

namespace DynamoDbSample
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			Console.WriteLine("Creating DynamoDb client");

			var client = CreateClient();

			var repo = new DynamoRepo<TestSample>(client, TestSample.TableName);

			Console.WriteLine("Creating DynamoDb repository");

			var entity = new TestSample()
			{
				Id = Guid.NewGuid(),
				EmptyStr = string.Empty
			};

			Console.WriteLine("Storing the item");

			await repo.Store(entity);

			Console.WriteLine("Getting the item");

			var fetchedEntity = await repo.Get(entity.Id.ToString());

			Console.WriteLine($"Fetched Entity. Id {fetchedEntity.Id}. EmptyStr {fetchedEntity.EmptyStr}");

			Console.WriteLine("Press any key to exit");

			Console.ReadKey();
		}

		private static AmazonDynamoDBClient CreateClient()
		{
			var localDynamo = Environment.GetEnvironmentVariable("AWS_LOCAL_DYNAMO");

			if (string.IsNullOrWhiteSpace(localDynamo))
			{
				// this should use aws default env variables
				return new AmazonDynamoDBClient();
			}

			return new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = localDynamo });
		}
	}
}