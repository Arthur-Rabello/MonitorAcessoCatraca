using System;

namespace MonitorAcessoCatraca.Models
{
    public class AcessoAutomatico
    {
        public int? CodigoCliente { get; set; }
        public string NomeCliente { get; set; }
        public bool Liberado { get; set; }
        public string Mensagem { get; set; }
        public string Motivo { get; set; }
        public string Servico { get; set; }
        public DateTime DataHora { get; set; }

        public string StatusTexto
        {
            get { return Liberado ? "LIBERADO" : "BLOQUEADO"; }
        }

        public string TituloNotificacao
        {
            get { return Liberado ? "ACESSO LIBERADO" : "ACESSO BLOQUEADO"; }
        }

        public string MotivoTratado
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Motivo))
                    return Motivo;

                if (!string.IsNullOrWhiteSpace(Mensagem))
                    return Mensagem;

                if (Liberado)
                    return "Acesso autorizado.";

                return "Acesso não autorizado.";
            }
        }

        public string ServicoTratado
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Servico))
                    return "Não informado";

                return Servico;
            }
        }
    }
}