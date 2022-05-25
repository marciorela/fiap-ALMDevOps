using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Trabalho_ALM
{
    public class Cliente
    {

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Key { get; set; }

        private List<Voucher> _vouchers = new List<Voucher>();
        public List<Voucher> Vouchers => _vouchers;
    }
}
