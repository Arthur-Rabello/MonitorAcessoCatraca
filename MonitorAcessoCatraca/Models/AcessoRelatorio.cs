using System;

namespace MonitorAcessoCatraca.Models
{
    public class AcessoRelatorio
    {
        public long Id { get; set; }
        public long CodigoContratoClienteAcesso { get; set; }
        public string NomeCliente { get; set; }
        public string Contrato { get; set; }
        public DateTime DataHora { get; set; }
        public bool Liberado { get; set; }
        public string Motivo { get; set; }

        public long ObterIdentificador()
        {
            if (CodigoContratoClienteAcesso > 0)
                return CodigoContratoClienteAcesso;

            return Id;
        }
    }
}