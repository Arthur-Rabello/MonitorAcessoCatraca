using System;
using System.Collections.Generic;

namespace MonitorAcessoCatraca.DTOs
{
    public class AcessoRelatorioDto
    {
        public List<AcessoRelatorioItemDto> Content { get; set; }
        public bool Success { get; set; }
        public int Total { get; set; }
    }

    public class AcessoRelatorioItemDto
    {
        public long Id { get; set; }
        public long CodigoContratoClienteAcesso { get; set; }
        public string NomeCliente { get; set; }
        public string DescricaoContrato { get; set; }
        public DateTime? DataHora { get; set; }
        public bool AcessoLiberado { get; set; }
        public string Motivo { get; set; }
        public int? TipoMotivo { get; set; }
    }
}