using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace DB
{
    /**
    Métodos de Acesso ao banco de dados CosmosDB
    */
    public class DBCosmos
    {
        /// The Azure Cosmos DB endpoint for running this GetStarted sample.
        private string EndpointUrl = Environment.GetEnvironmentVariable("EndpointUrl");

        /// The primary key for the Azure DocumentDB account.
        private string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "SumDatabase";
        private string containerId = "SumContainer";

        public async Task InitDBCosmos()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUrl, PrimaryKey);
            await CreateDatabaseAsync();
            await CreateContainerAsync();
        }

        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/Key");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        public async Task AddValueToContainerAsync(int v)
        {

            Console.WriteLine("AddValueToContainerAsync - 1");
            SumValue SValue = new SumValue();
            SValue.Sum = v;
            SValue.Id = "1";
            SValue.Key = "sum";

            int value = await QueryItemsAsync();

            if (value <= 0)
            {
                try
                {
                    Console.WriteLine("AddValueToContainerAsync - 2");
                    ItemResponse<SumValue> sumResponse = await this.container.CreateItemAsync<SumValue>(SValue,new PartitionKey(SValue.Key));
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", sumResponse.Resource.Id, sumResponse.RequestCharge);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    Console.WriteLine("Item in database with id: {0} already exists\n", SValue.Id);
                }
                return;
            }
            Console.WriteLine("AddValueToContainerAsync - 3");
            await ReplaceFamilyItemAsync(v);

        }

        private async Task ReplaceFamilyItemAsync(int v)
        {
            SumValue SValue = new SumValue();
            SValue.Id = "1";
            SValue.Sum = v;
            SValue.Key ="sum";

            ItemResponse<SumValue> itemSumResponse;

            Console.WriteLine("ReplaceFamilyItemAsync - 1");
            
            // replace the item with the updated content
            itemSumResponse = await this.container.ReplaceItemAsync<SumValue>(SValue, SValue.Id, new PartitionKey(SValue.Key));
            Console.WriteLine("Updated Sum.\n \tIs now: {0}\n", SValue.Sum);
        }

        public async Task<int> QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.Key = 'sum'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<SumValue> queryResultSetIterator = this.container.GetItemQueryIterator<SumValue>(queryDefinition);

            List<SumValue> values = new List<SumValue>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<SumValue> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (SumValue family in currentResultSet)
                {
                    values.Add(family);
                    Console.WriteLine("\tRead {0}\n", family);
                }
            }
            if (values.Count > 0)
            {
                return values[0].Sum;
            }
            return 0;
        }
    }
}