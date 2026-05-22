namespace MonitorAcessoCatraca.Config
{
    public static class AppConfig
    {
        public const string PastaControleAcesso = @"C:\Program Files (x86)\Next Fit\Controle de acesso";
        public const string CaminhoBanco = @"C:\Program Files (x86)\Next Fit\Controle de acesso\banco.db3";

        public const string NomeProcessoControleAcesso = "ControleAcesso";
        public const string NomeExecutavelControleAcesso = "ControleAcesso.exe";

        public const string HostAcesso = "acesso.nextfit.com.br";
        public const string EndpointAcessoAutomatico = "/api/v1/ContratoClienteAcesso/AcessoAutomatico";

        public const int PortaProxy = 8877;
    }
}