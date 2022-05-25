using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace Trabalho_ALM
{
    public class FunctionsVoucher
    {
        CosmosDB cosmosDB = new CosmosDB();

        private static Trabalho_ALM.CosmosDB dBCosmos;
        /**
        Inicialização do CosmosDB
        */
        private static async Task initDB()
        {
            if (dBCosmos == null)
            {
                dBCosmos = new Trabalho_ALM.CosmosDB();
                await dBCosmos.InitDBCosmos();
            }
        }

        [FunctionName("SolicitaVoucher")]
        public async Task<IActionResult> SolicitaVoucher(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                await initDB();
                Cliente cliente = new Cliente();
                Voucher voucher = new Voucher();

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                var nome = data?.nome.ToString();
                var cpf = data?.cpf.ToString();

                string codigoVoucher = RandomString(5);
                voucher.Id = Guid.NewGuid();
                voucher.Codigo = codigoVoucher;
                voucher.Validado = 0;
                voucher.Key = "sum";                

                cliente = await getCliente(cpf);
                cliente.Vouchers.Add(voucher);
                if (cliente.Cpf == null)
                {
                    cliente.Id = Guid.NewGuid();
                    cliente.Nome = nome;
                    cliente.Cpf = cpf;
                    JsonConvert.SerializeObject(cliente);
                    int result = await dBCosmos.CriaVoucher(cliente);
                }

                else
                {
                    JsonConvert.SerializeObject(cliente);
                    int result = await dBCosmos.AtualizarClienteItemAsync(cliente);
                }

                return new OkObjectResult(new
                {
                    id = Guid.NewGuid(),
                    nome = nome,
                    cpf = cpf,
                    voucher = codigoVoucher
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    id = "Erro"

                });
            }
        }

        [FunctionName("ValidaVoucher")]
        public async Task<IActionResult> ValidaVoucher(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            await initDB();
            Cliente cliente = new Cliente();
            int result = 0;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var cpf = data?.cpf.ToString();
            var codigo = data?.codigo.ToString();

            cliente = await getCliente(cpf);
            if (cliente.Cpf != null)
            {
                Voucher voucher = await dBCosmos.QueryItemsVoucherAsync(codigo);
                if (voucher.Id != null)
                {
                    //voucher.Validado = 1;
                    //result = await dBCosmos.ValidaVoucher(voucher);
                    cliente.Vouchers.Where(v => v.Codigo == codigo).ToList().ForEach(i => i.Validado = 1);
                    JsonConvert.SerializeObject(cliente);
                    result = await dBCosmos.AtualizarClienteItemAsync(cliente);
                }                
            }
            

            if(result == 1)
                return new OkObjectResult($"Voucher validado com sucesso.");
            else
                return new OkObjectResult($"Erro ao validar Voucher.");
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static async Task<Cliente> getCliente(string cpf)
        {
            Cliente cliente = new Cliente();
            try
            {
                int result = 0;
                await initDB();
                cliente = await dBCosmos.QueryItemsClienteAsync(cpf);
                return cliente;
            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                return cliente;
            }
        }

    }

}
