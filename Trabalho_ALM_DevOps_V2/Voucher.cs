using Newtonsoft.Json;
using System;

namespace Trabalho_ALM
{
    public class Voucher
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        public string Codigo { get; set; }
        public int Validado { get; set; }
        public string Key { get; set; }

    }
}
