using Newtonsoft.Json;

namespace MonitorAcessoCatraca.DTOs
{
    public class AcessoManualRequestDto
    {
        [JsonProperty("CodigoCliente")]
        public long? CodigoCliente { get; set; }

        [JsonProperty("Motivo")]
        public string Motivo { get; set; }
    }
}