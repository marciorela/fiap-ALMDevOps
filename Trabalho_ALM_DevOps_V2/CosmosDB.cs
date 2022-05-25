using System;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace Trabalho_ALM
{
    public class CosmosDB
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

        public async void SaveDB()
        {

        }

        public async void CriaVoucher(Voucher voucher)
        {
            try
            {
                ItemResponse<Voucher> voucherResponse = await this.container.CreateItemAsync<Voucher>(voucher, new PartitionKey(voucher.Key));
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", voucherResponse.Resource.Id, voucherResponse.RequestCharge);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Item in database with id: {0} already exists\n", voucher.Id);
            }
        }

        public async Task<int> CriaVoucher(Cliente cliente)
        {
            try
            {
                ItemResponse<Cliente> voucherResponse = await this.container.CreateItemAsync<Cliente>(cliente, new PartitionKey(cliente.Key));
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", voucherResponse.Resource.Id, voucherResponse.RequestCharge);
                return 1;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Item in database with id: {0} already exists\n", cliente.Id);
                return 0;
            }
        }

        internal async Task<int> ValidaVoucher(Voucher voucher)
        {
            try
            {
                ItemResponse<Voucher> itemSumResponse;
                // replace the item with the updated content
                itemSumResponse = await this.container.ReplaceItemAsync<Voucher>(voucher, voucher.Id.ToString(), new PartitionKey(voucher.Key));
                return 1;
            }
            catch (Exception ex)
            { return 0; }
        }
    

        public async Task<Cliente> QueryItemsClienteAsync(string cpf)
        {
            Cliente cli = new Cliente();
            try
            {
                var sqlQueryText = "SELECT * FROM c where c.Cpf = '" + cpf + "'";

                Console.WriteLine("Running query: {0}\n", sqlQueryText);

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Cliente> queryResultSetIterator = this.container.GetItemQueryIterator<Cliente>(queryDefinition);

                List<Cliente> values = new List<Cliente>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Cliente> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Cliente family in currentResultSet)
                    {
                        values.Add(family);
                        Console.WriteLine("\tRead {0}\n", family);
                    }
                }
                if (values.Count > 0)
                {
                    return values[0];
                    //return 1;
                }
                return cli;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return cli;
            }
        }

        public async Task<Voucher> QueryItemsVoucherAsync(string codigo)
        {
            Voucher voucher = new Voucher();
            try
            {
                var sqlQueryText = "SELECT * FROM c IN t.Vouchers WHERE c.Codigo = '"+ codigo + "'";

                Console.WriteLine("Running query: {0}\n", sqlQueryText);

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Voucher> queryResultSetIterator = this.container.GetItemQueryIterator<Voucher>(queryDefinition);

                List<Voucher> values = new List<Voucher>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Voucher> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Voucher family in currentResultSet)
                    {
                        values.Add(family);
                        Console.WriteLine("\tRead {0}\n", family);
                    }
                }
                if (values.Count > 0)
                {
                    return values[0];
                    //return 1;
                }
                return voucher;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return voucher;
            }
        }

        public async Task<int> QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.Key = 'voucher'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Voucher> queryResultSetIterator = this.container.GetItemQueryIterator<Voucher>(queryDefinition);

            List<Voucher> values = new List<Voucher>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Voucher> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Voucher family in currentResultSet)
                {
                    values.Add(family);
                    Console.WriteLine("\tRead {0}\n", family);
                }
            }
            if (values.Count > 0)
            {
                //return values[0].Sum;
                return 1;
            }
            return 0;
        }

        public async Task<int> AtualizarClienteItemAsync(Cliente cliente)
        {
            try
            {
                ItemResponse<Cliente> itemSumResponse;
                // replace the item with the updated content
                itemSumResponse = await this.container.ReplaceItemAsync<Cliente>(cliente, cliente.Id.ToString(), new PartitionKey(cliente.Key));
                return 1;
            }
            catch (Exception ex)
            { return 0; }
        }
    }
}
