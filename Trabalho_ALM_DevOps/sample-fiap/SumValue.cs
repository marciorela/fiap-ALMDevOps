using Newtonsoft.Json;

/**
Estrutura para armazenar a soma no Banco de Dados
*/
public class SumValue
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    public int Sum { get; set; }
    public string Key { get; set; }
}