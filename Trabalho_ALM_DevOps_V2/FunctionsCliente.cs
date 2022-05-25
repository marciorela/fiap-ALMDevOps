using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Trabalho_ALM
{
    public class FunctionsCliente
    {
        CosmosDB cosmosDB = new CosmosDB();
        [FunctionName("CadastroCliente")]
        public async Task<IActionResult> CadastroCliente(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            var email = data?.email;
            var nome = data?.nome;

            cosmosDB.SaveDB();

            return new OkObjectResult(
                $"Cliente {email}, {nome} cadastrado com sucesso!"
            );
        }
    }
}
