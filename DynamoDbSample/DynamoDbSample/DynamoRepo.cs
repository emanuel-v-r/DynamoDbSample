using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbSample
{
	public class DynamoRepo<TDbEntity>
		where TDbEntity : class
	{
		private readonly Table _table;

		public DynamoRepo(IAmazonDynamoDB dynamoDb, string tableName)
		{
			// GetResult() hammer in sake of simplification
			CreateTableIfNotExists(dynamoDb, tableName).GetAwaiter().GetResult();

			_table = Table.LoadTable(dynamoDb, tableName);
		}

		public async Task<TDbEntity> Get(string key)
		{
			var item = await _table.GetItemAsync(key).ConfigureAwait(false);

			if (item == null)
			{
				return null;
			}

			var json = item.ToJson();

			// HERE'S THE Empty String issue
			Console.WriteLine($"Retrieved Json {json}. This should come with an empty string");

			return JsonSerializer.Deserialize<TDbEntity>(json, JsonSerialization.Settings);
		}

		public Task Store(TDbEntity entity)
		{
			var jsonText = JsonSerializer.Serialize(entity, JsonSerialization.Settings);

			// HERE we can verify that the json includes the emptyString
			Console.WriteLine($"Stored Json{jsonText}");

			var item = Document.FromJson(jsonText);

			return _table.PutItemAsync(item);
		}

		private async Task CreateTableIfNotExists(IAmazonDynamoDB dynamoDb, string tableName)
		{
			try
			{
				await dynamoDb.CreateTableAsync(
						new CreateTableRequest()
						{
							TableName = tableName,
							AttributeDefinitions = new List<AttributeDefinition>()
							{
							new AttributeDefinition
							{
								AttributeName = "id",
								AttributeType = ScalarAttributeType.S
							},
							},
							KeySchema = new List<KeySchemaElement>
							{
							new KeySchemaElement
							{
								AttributeName = "id",
								KeyType = KeyType.HASH
							}
							},
							BillingMode = BillingMode.PAY_PER_REQUEST
						});

				await WaitUntilTableReady(dynamoDb, tableName);
			}
			catch (ResourceInUseException)
			{
				Console.WriteLine($"Table {tableName} already exists");
			}
		}

		private static async Task WaitUntilTableReady(IAmazonDynamoDB client, string tableName)
		{
			string status = null;
			// Let us wait until table is created. Call DescribeTable.
			do
			{
				Console.WriteLine($"Waiting for table {tableName} to be active");

				await Task.Delay(5000);// Wait 5 seconds.

				try
				{
					var res = await client.DescribeTableAsync(new DescribeTableRequest
					{
						TableName = tableName
					});
					Console.WriteLine("Table name: {0}, status: {1}",
									res.Table.TableName,
									res.Table.TableStatus);
					status = res.Table.TableStatus;
				}
				catch (ResourceNotFoundException)
				{
					// DescribeTable is eventually consistent. So you might
					// get resource not found. So we handle the potential exception.
				}
			} while (status != TableStatus.ACTIVE);
		}
	}
}