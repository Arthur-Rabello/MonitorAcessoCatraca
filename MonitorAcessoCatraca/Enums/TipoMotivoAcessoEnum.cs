using System.ComponentModel;

namespace MonitorAcessoCatraca.Enums
{
    public enum TipoMotivoAcessoEnum
    {
        [Description("Acesso manual")]
        Manual = 0,

        [Description("Cliente sem contrato ativo")]
        ClienteSemContratoAtivo = 1,

        [Description("Contrato sem modalidade")]
        ContratoSemModalidade = 2,

        [Description("Contrato sem sessão disponível")]
        ContratoSemSessaoDisponivel = 3,

        [Description("Contrato sem aula no dia")]
        ContratoSemAulaNoDia = 4,

        [Description("Contrato fora do dia permitido")]
        ContratoForaDoDiaPermitido = 5,

        [Description("Contrato fora do horário permitido")]
        ContratoForaDoHorarioPermitido = 6,

        [Description("Quantidade de acessos na semana atingida")]
        ContratoQtdeDeAcessosSemanaAtingido = 7,

        [Description("Quantidade de acessos no período atingida")]
        ContratoQtdeDeAcessosPeriodoAtingido = 8
    }
}