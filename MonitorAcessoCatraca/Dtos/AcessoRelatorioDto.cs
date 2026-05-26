using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MonitorAcessoCatraca.DTOs
{
    public class AcessoRelatorioDto
    {
        [JsonProperty("Content")]
        public JToken Content { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }

        public List<AcessoRelatorioItemDto> ObterListaContent()
        {
            if (Content == null)
                return new List<AcessoRelatorioItemDto>();

            if (Content.Type == JTokenType.Array)
                return Content.ToObject<List<AcessoRelatorioItemDto>>();

            if (Content.Type == JTokenType.Object)
            {
                AcessoRelatorioItemDto item = Content.ToObject<AcessoRelatorioItemDto>();

                if (item != null)
                    return new List<AcessoRelatorioItemDto> { item };
            }

            return new List<AcessoRelatorioItemDto>();
        }

        public AcessoRelatorioItemDto ObterPrimeiroContent()
        {
            List<AcessoRelatorioItemDto> lista = ObterListaContent();

            if (lista == null || lista.Count == 0)
                return null;

            return lista[0];
        }
    }

    public class AcessoRelatorioItemDto
    {
        public int Id { get; set; }
        public int CodigoContratoClienteAcesso { get; set; }

        public int? CodigoCliente { get; set; }
        public int? CodigoContratoCliente { get; set; }
        public int? CodigoContratoClienteModalidade { get; set; }
        public int? CodigoUsuarioCriacao { get; set; }

        public string NomeCliente { get; set; }
        public string DescricaoContrato { get; set; }

        public DateTime? DataHora { get; set; }

        public bool AcessoLiberado { get; set; }

        public string Motivo { get; set; }

        public int? TipoMotivo { get; set; }
        public int? Tipo { get; set; }
        public int? ModoReconhecimento { get; set; }

        public bool Offline { get; set; }
    }
}