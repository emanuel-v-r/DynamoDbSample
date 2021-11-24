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

			Console.WriteLine($"Retrieved Json {json}. This should come with an empty string");

			return JsonSerializer.Deserialize<TDbEntity>(json, JsonSerialization.Settings);
		}

		public Task Store(TDbEntity entity)
		{
			var jsonText = JsonSerializer.Serialize(entity, JsonSerialization.Settings);

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
			}
			catch (ResourceInUseException)
			{
				Console.WriteLine($"Table {tableName} already exists");
			}
		}
	}
}