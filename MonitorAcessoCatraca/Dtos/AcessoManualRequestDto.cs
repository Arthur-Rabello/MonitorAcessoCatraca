using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonitorAcessoCatraca.DTOs
{
    public class AcessoManualRequestDto
    {
        [JsonProperty("CodigoCliente")]
        public long? CodigoCliente { get; set; }

        [JsonProperty("CodigoEquipamento")]
        public int? CodigoEquipamento { get; set; }

        [JsonProperty("TemEquipamentos")]
        public bool TemEquipamentos { get; set; } = true;

        [JsonProperty("Equipamentos")]
        public List<EquipamentoDto> Equipamentos { get; set; } = new List<EquipamentoDto>();

        [JsonProperty("Equipamento")]
        public EquipamentoDto Equipamento { get; set; }
    }

    public class EquipamentoDto
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Descricao")]
        public string Descricao { get; set; }

        [JsonProperty("Offline")]
        public bool Offline { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }
}