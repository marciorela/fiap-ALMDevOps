using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace My.Function
{
    public static class FunctionExample
    {
        /**
        Função GET que retorna o valor da soma armazenado no CosmosDB
        */
        [FunctionName("GetSum")]
        public static async Task<IActionResult> GetSum(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = "Valor da soma armazenado eh: " + await getSum();

            return new OkObjectResult(responseMessage);
        }

        /**
        Função POST que recupera a soma no Cosmos DB e adiciona um novo valor.
        exemplo de body para o post:
        { "value": "4" }
        */
        [FunctionName("Add")]
        public static async Task<IActionResult> Add(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string value = data?.value;

            string responseMessage = "Somado com sucesso";

            if(string.IsNullOrEmpty(value))
            {
                responseMessage = "vazio";
            }

            int iValue = await getSum();

            iValue = iValue+int.Parse(value);

            await setSum(iValue);
            
            return new OkObjectResult(responseMessage);
        }

        private static DB.DBCosmos dBCosmos;
        /**
        Inicialização do CosmosDB
        */
        private static async Task initDB()
        {
            if(dBCosmos==null)
            {
                dBCosmos = new DB.DBCosmos();
                await dBCosmos.InitDBCosmos();
            }
        }
        
        /**
        Retorna o valor da soma armazenado no CosmosDB
        */
        private static async Task<int> getSum()
        {
            int result = 0;
            await initDB();
            result = await dBCosmos.QueryItemsAsync();
            return result;
        }

        /**
        Grava o valor da soma no CosmosDB
        */
        private static async Task setSum(int v)
        {
            await initDB();
            await dBCosmos.AddValueToContainerAsync(v);
        }
    }
    
}
