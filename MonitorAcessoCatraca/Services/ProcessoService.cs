using MonitorAcessoCatraca.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MonitorAcessoCatraca.Services
{
    public class ProcessoService
    {
        private bool jaTentouAbrirControleAcesso = false;

        public bool ControleAcessoEstaAberto()
        {
            return Process
                .GetProcessesByName(AppConfig.NomeProcessoControleAcesso)
                .Any();
        }

        public bool TentarAbrirControleAcesso(out string mensagem)
        {
            mensagem = "";

            try
            {
                if (ControleAcessoEstaAberto())
                {
                    mensagem = "Controle de Acesso já está aberto.";
                    return true;
                }

                if (jaTentouAbrirControleAcesso)
                {
                    mensagem = "Controle de Acesso está fechado, mas já foi tentado abrir anteriormente. Não será aberto novamente automaticamente.";
                    return false;
                }

                string caminhoExe = Path.Combine(
                    AppConfig.PastaControleAcesso,
                    AppConfig.NomeExecutavelControleAcesso
                );

                if (!File.Exists(caminhoExe))
                {
                    mensagem = "Não foi possível encontrar: " + caminhoExe;
                    return false;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = caminhoExe,
                    WorkingDirectory = AppConfig.PastaControleAcesso,
                    UseShellExecute = true
                };

                Process.Start(startInfo);

                jaTentouAbrirControleAcesso = true;

                mensagem = "Controle de Acesso iniciado: " + caminhoExe;
                return true;
            }
            catch (Exception ex)
            {
                jaTentouAbrirControleAcesso = true;

                mensagem = "Erro ao abrir Controle de Acesso: " + ex.Message;
                return false;
            }
        }

        public void PermitirAbrirNovamente()
        {
            jaTentouAbrirControleAcesso = false;
        }
    }
}