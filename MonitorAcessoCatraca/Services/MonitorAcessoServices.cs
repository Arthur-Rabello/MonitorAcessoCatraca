using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.DTOs;
using MonitorAcessoCatraca.Forms;
using MonitorAcessoCatraca.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Services
{
    public class MonitorAcessoService
    {
        private readonly FormPrincipal form;

        private FormPrincipalControles controles;

        private ProcessoService processoService;
        private ProxyInterceptacaoService proxyService;
        private RelatorioAcessoService relatorioAcessoService;
        private ConfiguracaoService configuracaoService;
        private NextFitAuthService nextFitAuthService;
        private AcessoManualService acessoManualService;
        private BandejaService bandejaService;
        private HotkeyGlobalService hotkeyService;
        private FormPrincipalLayoutService layoutService;

        private const int HOTKEY_LIBERAR_ACESSO = 0;

        private bool encerrandoAplicacao = false;
        private bool proxyIniciado = false;
        private bool paradoManualmente = false;
        private bool consultandoRelatorio = false;
        private bool liberandoAcessoManual = false;

        private long ultimoAcessoNotificado = 0;

        public MonitorAcessoService(FormPrincipal form)
        {
            this.form = form;
        }

        public void Inicializar()
        {
            layoutService = new FormPrincipalLayoutService();
            controles = layoutService.Montar(form);

            processoService = new ProcessoService();
            relatorioAcessoService = new RelatorioAcessoService();
            acessoManualService = new AcessoManualService();
            configuracaoService = new ConfiguracaoService();
            nextFitAuthService = new NextFitAuthService(new HttpClient());

            proxyService = new ProxyInterceptacaoService();
            proxyService.AcessoAutomaticoDetectado += ProxyService_AcessoAutomaticoDetectado;

            controles.BtnIniciar.Click += BtnIniciar_Click;
            controles.BtnParar.Click += BtnParar_Click;

            controles.TimerVerificarControleAcesso.Tick += TimerVerificarControleAcesso_Tick;
            controles.TimerVerificarControleAcesso.Start();

            ConfigurarServicosInterface();

            InicializacaoWindowsService.AtivarInicializacaoComWindows();

            AdicionarLog("Monitor iniciado.");
            AdicionarLog("Aguardando Controle de Acesso abrir...");
            controles.LblStatus.Text = "Status: aguardando Controle de Acesso...";
        }

        private void ConfigurarServicosInterface()
        {
            bandejaService = new BandejaService();

            bandejaService.AbrirSolicitado += AbrirMonitor;

            bandejaService.IniciarSolicitado += () =>
            {
                paradoManualmente = false;
                IniciarMonitoramento();
            };

            bandejaService.PararSolicitado += PararMonitoramento;
            bandejaService.SairSolicitado += SairAplicacao;

            bandejaService.Configurar();

            hotkeyService = new HotkeyGlobalService(
                HOTKEY_LIBERAR_ACESSO,
                Keys.F9,
                false,
                false,
                false
            );

            hotkeyService.AtalhoPressionado += () =>
            {
                AdicionarLog("Atalho " + hotkeyService.ObterTextoAtalho() + " acionado.");
                _ = LiberarAcessoManualAsync();
            };
        }

        public void AoExibirTela()
        {
            form.Hide();
            form.ShowInTaskbar = false;

            if (bandejaService != null)
            {
                bandejaService.MostrarMensagem(
                    "Monitor de Acessos",
                    "O monitor está rodando em segundo plano.",
                    ToolTipIcon.Info
                );
            }
        }

        public void AoCriarHandle()
        {
            RegistrarHotkeyNoHandle();
        }

        public void AoDestruirHandle()
        {
            try
            {
                if (hotkeyService != null)
                    hotkeyService.Desregistrar(form.Handle);
            }
            catch
            {
            }
        }

        public bool ProcessarMensagemWindows(Message mensagem)
        {
            if (hotkeyService != null && hotkeyService.ProcessarMensagem(mensagem))
                return true;

            return false;
        }

        public void AoFecharFormulario(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !encerrandoAplicacao)
            {
                e.Cancel = true;

                form.Hide();
                form.ShowInTaskbar = false;

                if (bandejaService != null)
                {
                    bandejaService.MostrarMensagem(
                        "Monitor de Acessos",
                        "O monitor continuará rodando em segundo plano.",
                        ToolTipIcon.Info
                    );
                }

                return;
            }

            EncerrarRecursos();
        }

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            paradoManualmente = false;
            IniciarMonitoramento();
        }

        private void BtnParar_Click(object sender, EventArgs e)
        {
            PararMonitoramento();
        }

        private void RegistrarHotkeyNoHandle()
        {
            if (hotkeyService == null)
                return;

            if (!form.IsHandleCreated)
                return;

            int erro;
            bool registrado = hotkeyService.Registrar(form.Handle, out erro);

            if (registrado)
            {
                AdicionarLog("Atalho " + hotkeyService.ObterTextoAtalho() + " registrado com sucesso.");
            }
            else
            {
                AdicionarLog(
                    "Não foi possível registrar o atalho " +
                    hotkeyService.ObterTextoAtalho() +
                    ". Erro Windows: " +
                    erro
                );
            }
        }

        private async Task LiberarAcessoManualAsync()
        {
            if (liberandoAcessoManual)
                return;

            liberandoAcessoManual = true;

            try
            {
                string textoAtalho = hotkeyService != null
                    ? hotkeyService.ObterTextoAtalho()
                    : "atalho configurado";

                AdicionarLog("Solicitando liberação manual pela tecla " + textoAtalho + "...");

                await ObterLoginAtualAsync();

                await acessoManualService.LiberarAcessoManualAsync(
                    nextFitAuthService.Token
                );

                AcessoAutomatico acessoManual = new AcessoAutomatico
                {
                    CodigoCliente = null,
                    NomeCliente = "Liberação manual",
                    Liberado = true,
                    Servico = "Acesso manual",
                    DataHora = DateTime.Now,
                    Motivo = "Via Monitor de Acesso",
                    Mensagem = "Acesso manual liberado"
                };

                AdicionarLog(FormatarAcesso(acessoManual));
                MostrarNotificacao(acessoManual);

                AdicionarLog("Acesso manual liberado com sucesso.");

                if (bandejaService != null)
                {
                    bandejaService.MostrarMensagem(
                        "Monitor de Acessos",
                        "Acesso manual liberado com sucesso.",
                        ToolTipIcon.Info
                    );
                }
            }
            catch (Exception ex)
            {
                AdicionarLog("Erro ao liberar acesso manual: " + ex.Message);

                if (bandejaService != null)
                {
                    bandejaService.MostrarMensagem(
                        "Erro ao liberar acesso",
                        ex.Message,
                        ToolTipIcon.Error
                    );
                }
            }
            finally
            {
                await Task.Delay(2000);
                liberandoAcessoManual = false;
            }
        }

        private void TimerVerificarControleAcesso_Tick(object sender, EventArgs e)
        {
            if (paradoManualmente)
                return;

            bool controleAberto = processoService.ControleAcessoEstaAberto();

            if (!controleAberto)
            {
                if (proxyIniciado)
                {
                    AdicionarLog("Controle de Acesso foi fechado. Parando monitoramento...");

                    try
                    {
                        if (proxyService != null)
                            proxyService.Parar();

                        ProxyWindowsService.ExecutarComandoDesativarProxy();
                        ProxyWindowsService.DesativarProxyWindows();
                    }
                    catch
                    {
                    }

                    proxyIniciado = false;

                    controles.BtnIniciar.Enabled = true;
                    controles.BtnParar.Enabled = false;
                }

                controles.LblStatus.Text = "Status: Controle de Acesso não está aberto. Tentando abrir...";

                string mensagem;
                bool abriu = processoService.TentarAbrirControleAcessoPorTarefa(out mensagem);

                AdicionarLog(mensagem);

                if (!abriu)
                {
                    AdicionarLog("Tentando abrir Controle de Acesso pelo método normal...");

                    abriu = processoService.TentarAbrirControleAcesso(out mensagem);

                    AdicionarLog(mensagem);
                }

                controles.LblStatus.Text = "Status: aguardando inicialização do Controle de Acesso...";
                return;
            }

            if (!proxyIniciado)
            {
                AdicionarLog("Controle de Acesso detectado. Iniciando monitoramento...");
                IniciarMonitoramento();
            }
        }

        private async void ProxyService_AcessoAutomaticoDetectado()
        {
            if (consultandoRelatorio)
                return;

            consultandoRelatorio = true;

            try
            {
                AdicionarLog("AcessoAutomatico detectado. Consultando último acesso no relatório...");

                await Task.Delay(1500);

                await ObterLoginAtualAsync();

                int codigoUnidade;

                if (!int.TryParse(nextFitAuthService.CodigoUnidade, out codigoUnidade))
                    throw new Exception("Código da unidade inválido: " + nextFitAuthService.CodigoUnidade);

                AcessoRelatorio ultimo = await relatorioAcessoService.BuscarUltimoAcessoAsync(
                    nextFitAuthService.Token,
                    codigoUnidade
                );

                if (ultimo == null)
                {
                    AdicionarLog("Relatório não retornou acesso recente.");
                    return;
                }

                long idAcesso = ultimo.ObterIdentificador();

                if (idAcesso <= 0)
                    return;

                if (idAcesso == ultimoAcessoNotificado)
                    return;

                ultimoAcessoNotificado = idAcesso;

                AcessoAutomatico acesso = new AcessoAutomatico
                {
                    CodigoCliente = null,
                    NomeCliente = ultimo.NomeCliente,
                    Liberado = ultimo.Liberado,
                    Servico = ultimo.Contrato,
                    DataHora = ultimo.DataHora,
                    Motivo = ultimo.Liberado
                        ? "Acesso autorizado"
                        : !string.IsNullOrWhiteSpace(ultimo.Motivo)
                            ? ultimo.Motivo
                            : "Acesso bloqueado",
                    Mensagem = ultimo.Liberado
                        ? "Acesso liberado"
                        : !string.IsNullOrWhiteSpace(ultimo.Motivo)
                            ? ultimo.Motivo
                            : "Acesso bloqueado"
                };

                AdicionarLog(FormatarAcesso(acesso));
                MostrarNotificacao(acesso);
            }
            catch (Exception ex)
            {
                AdicionarLog("Erro ao consultar último acesso: " + ex.Message);
            }
            finally
            {
                consultandoRelatorio = false;
            }
        }

        private async Task ObterLoginAtualAsync()
        {
            ConfiguracaoApiDto configuracao = configuracaoService.ObterConfiguracao();

            if (configuracao == null)
                throw new Exception("Configuração de login não encontrada.");

            if (string.IsNullOrWhiteSpace(configuracao.Email))
                throw new Exception("E-mail da configuração não encontrado.");

            if (string.IsNullOrWhiteSpace(configuracao.Senha))
                throw new Exception("Senha da configuração não encontrada.");

            await nextFitAuthService.LoginAsync(
                configuracao.Email,
                configuracao.Senha
            );

            if (string.IsNullOrWhiteSpace(nextFitAuthService.Token))
                throw new Exception("Token não retornado pela API.");

            if (string.IsNullOrWhiteSpace(nextFitAuthService.CodigoUnidade))
                throw new Exception("Código da unidade não retornado pela API.");

            await nextFitAuthService.AceitarTermosAsync();
        }

        private void IniciarMonitoramento()
        {
            try
            {
                if (proxyIniciado)
                    return;

                proxyService.Iniciar();

                proxyIniciado = true;
                paradoManualmente = false;

                controles.BtnIniciar.Enabled = false;
                controles.BtnParar.Enabled = true;
                controles.LblStatus.Text = "Status: monitorando comunicação do Controle de Acesso...";

                AdicionarLog("Proxy iniciado na porta " + AppConfig.PortaProxy + ".");
                AdicionarLog("Aguardando requisições de AcessoAutomatico...");

                if (bandejaService != null)
                {
                    bandejaService.MostrarMensagem(
                        "Monitor de Acessos",
                        "Monitoramento iniciado.",
                        ToolTipIcon.Info
                    );
                }
            }
            catch (Exception ex)
            {
                proxyIniciado = false;
                controles.LblStatus.Text = "Status: erro ao iniciar monitoramento";
                AdicionarLog("ERRO ao iniciar monitoramento: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Erro ao iniciar monitoramento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void PararMonitoramento()
        {
            paradoManualmente = true;

            try
            {
                if (proxyService != null)
                    proxyService.Parar();

                ProxyWindowsService.ExecutarComandoDesativarProxy();
                ProxyWindowsService.DesativarProxyWindows();
            }
            catch
            {
            }

            proxyIniciado = false;

            controles.BtnIniciar.Enabled = true;
            controles.BtnParar.Enabled = false;
            controles.LblStatus.Text = "Status: parado manualmente";

            AdicionarLog("Monitoramento parado manualmente. Proxy do Windows desativado.");
        }

        private string FormatarAcesso(AcessoAutomatico acesso)
        {
            return
                "[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "] " +
                acesso.StatusTexto +
                " | Cliente: " + acesso.NomeCliente +
                " | Serviço: " + acesso.ServicoTratado +
                " | Motivo: " + acesso.MotivoTratado;
        }

        private void MostrarNotificacao(AcessoAutomatico acesso)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() => MostrarNotificacao(acesso)));
                return;
            }

            FormNotificacaoAcesso notificacao = new FormNotificacaoAcesso(acesso);
            notificacao.Show();
        }

        private void AdicionarLog(string texto)
        {
            if (controles == null || controles.TxtLog == null)
                return;

            if (controles.TxtLog.InvokeRequired)
            {
                controles.TxtLog.Invoke(new Action(() => AdicionarLog(texto)));
                return;
            }

            controles.TxtLog.AppendText(texto + Environment.NewLine);
        }

        private void AbrirMonitor()
        {
            form.Show();
            form.WindowState = FormWindowState.Normal;
            form.ShowInTaskbar = true;
            form.BringToFront();
        }

        private void SairAplicacao()
        {
            encerrandoAplicacao = true;
            EncerrarRecursos();
            Application.Exit();
        }

        private void EncerrarRecursos()
        {
            try
            {
                if (hotkeyService != null)
                    hotkeyService.Desregistrar(form.Handle);
            }
            catch
            {
            }

            try
            {
                if (controles != null && controles.TimerVerificarControleAcesso != null)
                    controles.TimerVerificarControleAcesso.Stop();

                if (proxyService != null)
                    proxyService.Parar();

                ProxyWindowsService.ExecutarComandoDesativarProxy();
                ProxyWindowsService.DesativarProxyWindows();

                if (bandejaService != null)
                    bandejaService.Dispose();
            }
            catch
            {
            }
        }
    }
}