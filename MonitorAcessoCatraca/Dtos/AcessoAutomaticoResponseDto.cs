using System;

namespace MonitorAcessoCatraca.DTOs
{
    public class AcessoAutomaticoResponseDto
    {
        public AcessoAutomaticoContentDto Content { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class AcessoAutomaticoContentDto
    {
        public int? CodigoCliente { get; set; }
        public int? CodigoContratoCliente { get; set; }
        public DateTime? DataValidade { get; set; }
        public bool Erro { get; set; }
        public string Mensagem { get; set; }
        public string MotivoErro { get; set; }
        public decimal? ProximoValorReceber { get; set; }
        public DateTime? ProximoVencimentoReceber { get; set; }
        public string Servico { get; set; }
    }
}