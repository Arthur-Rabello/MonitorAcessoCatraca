using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.DTOs;
using MonitorAcessoCatraca.Models;
using MonitorAcessoCatraca.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.IO;

namespace MonitorAcessoCatraca.Forms
{
    public class FormPrincipal : Form
    {
        private Button btnIniciar;
        private Button btnParar;
        private TextBox txtLog;
        private Label lblStatus;
        private Label lblInfo;

        private Timer timerVerificarControleAcesso;

        private NotifyIcon notifyIcon;
        private ContextMenuStrip menuBandeja;
        private bool encerrandoAplicacao = false;
        private ProcessoService processoService;
        private ProxyInterceptacaoService proxyService;
        private RelatorioAcessoService relatorioAcessoService;
        private ConfiguracaoService configuracaoService;
        private NextFitAuthService nextFitAuthService;

        private bool proxyIniciado = false;
        private bool paradoManualmente = false;
        private bool consultandoRelatorio = false;
        private long ultimoAcessoNotificado = 0;

        public FormPrincipal()
        {
            MontarTela();
        }

        private void MontarTela()
        {
            Text = "Monitor de Acessos - Next Fit";
            Width = 850;
            Height = 560;
            StartPosition = FormStartPosition.CenterScreen;

            lblInfo = new Label
            {
                Text = "Monitorando: " + AppConfig.HostAcesso + " / " + AppConfig.EndpointAcessoAutomatico,
                Left = 20,
                Top = 25,
                Width = 790,
                Height = 25
            };

            Controls.Add(lblInfo);

            btnIniciar = new Button
            {
                Text = "Iniciar monitoramento",
                Left = 20,
                Top = 65,
                Width = 180,
                Height = 35
            };

            btnIniciar.Click += BtnIniciar_Click;
            Controls.Add(btnIniciar);

            btnParar = new Button
            {
                Text = "Parar",
                Left = 210,
                Top = 65,
                Width = 100,
                Height = 35,
                Enabled = false
            };

            btnParar.Click += BtnParar_Click;
            Controls.Add(btnParar);

            lblStatus = new Label
            {
                Text = "Status: parado",
                Left = 330,
                Top = 75,
                Width = 470,
                Height = 25
            };

            Controls.Add(lblStatus);

            txtLog = new TextBox
            {
                Left = 20,
                Top = 120,
                Width = 790,
                Height = 370,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            Controls.Add(txtLog);

            processoService = new ProcessoService();
            relatorioAcessoService = new RelatorioAcessoService();
            configuracaoService = new ConfiguracaoService();
            nextFitAuthService = new NextFitAuthService(new HttpClient());

            proxyService = new ProxyInterceptacaoService();
            proxyService.AcessoAutomaticoDetectado += ProxyService_AcessoAutomaticoDetectado;

            timerVerificarControleAcesso = new Timer();
            timerVerificarControleAcesso.Interval = 5000;
            timerVerificarControleAcesso.Tick += TimerVerificarControleAcesso_Tick;
            timerVerificarControleAcesso.Start();

            ConfigurarBandeja();


            InicializacaoWindowsService.AtivarInicializacaoComWindows();
            AdicionarLog("Monitor iniciado.");
            AdicionarLog("Aguardando Controle de Acesso abrir...");
            lblStatus.Text = "Status: aguardando Controle de Acesso...";
        }

        private void ConfigurarBandeja()
        {
            menuBandeja = new ContextMenuStrip();

            ToolStripMenuItem abrirItem = new ToolStripMenuItem("Abrir monitor");
            abrirItem.Click += (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
                BringToFront();
            };

            ToolStripMenuItem iniciarItem = new ToolStripMenuItem("Iniciar monitoramento");
            iniciarItem.Click += (s, e) =>
            {
                paradoManualmente = false;
                IniciarProxy();
            };

            ToolStripMenuItem pararItem = new ToolStripMenuItem("Parar monitoramento");
            pararItem.Click += (s, e) =>
            {
                PararMonitoramento();
            };

            ToolStripMenuItem sairItem = new ToolStripMenuItem("Sair");
            sairItem.Click += (s, e) =>
            {
                encerrandoAplicacao = true;

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

                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }

                Application.Exit();
            };

            menuBandeja.Items.Add(abrirItem);
            menuBandeja.Items.Add(new ToolStripSeparator());
            menuBandeja.Items.Add(iniciarItem);
            menuBandeja.Items.Add(pararItem);
            menuBandeja.Items.Add(new ToolStripSeparator());
            menuBandeja.Items.Add(sairItem);

            notifyIcon = new NotifyIcon();

            string caminhoIcone = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "MonitorAcessoCatraca_novo.ico"
            );

            if (!File.Exists(caminhoIcone))
            {
                caminhoIcone = Path.GetFullPath(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\MonitorAcessoCatraca_novo.ico"
                ));
            }

            if (File.Exists(caminhoIcone))
            {
                notifyIcon.Icon = new Icon(caminhoIcone);
            }
            else
            {
                notifyIcon.Icon = SystemIcons.Application;
            }

            notifyIcon.Text = "Monitor de Acessos - Next Fit";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = menuBandeja;

            notifyIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
                BringToFront();
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Hide();
            ShowInTaskbar = false;

            if (notifyIcon != null)
            {
                notifyIcon.BalloonTipTitle = "Monitor de Acessos";
                notifyIcon.BalloonTipText = "O monitor está rodando em segundo plano.";
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.ShowBalloonTip(3000);
            }
        }

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            paradoManualmente = false;
            IniciarProxy();
        }

        private void BtnParar_Click(object sender, EventArgs e)
        {
            PararMonitoramento();
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
                    AdicionarLog("Controle de Acesso foi fechado. Parando proxy...");

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

                    btnIniciar.Enabled = true;
                    btnParar.Enabled = false;
                }

                lblStatus.Text = "Status: Controle de Acesso não está aberto. Tentando abrir...";

                string mensagem;
                bool abriu = processoService.TentarAbrirControleAcessoPorTarefa(out mensagem);

                AdicionarLog(mensagem);

                if (!abriu)
                {
                    AdicionarLog("Tentando abrir Controle de Acesso pelo método normal...");

                    abriu = processoService.TentarAbrirControleAcesso(out mensagem);

                    AdicionarLog(mensagem);
                }

                lblStatus.Text = "Status: aguardando inicialização do Controle de Acesso...";
                return;
            }

            if (!proxyIniciado)
            {
                AdicionarLog("Controle de Acesso detectado. Iniciando proxy...");
                IniciarProxy();
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

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        AdicionarLog(FormatarAcesso(acesso));
                        MostrarNotificacao(acesso);
                    }));
                }
                else
                {
                    AdicionarLog(FormatarAcesso(acesso));
                    MostrarNotificacao(acesso);
                }
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

        private void IniciarProxy()
        {
            try
            {
                if (proxyIniciado)
                    return;

                proxyService.Iniciar();
                proxyIniciado = true;
                paradoManualmente = false;

                btnIniciar.Enabled = false;
                btnParar.Enabled = true;
                lblStatus.Text = "Status: monitorando comunicação do Controle de Acesso...";

                AdicionarLog("Proxy iniciado na porta " + AppConfig.PortaProxy + ".");
                AdicionarLog("Aguardando requisições de AcessoAutomatico...");

                if (notifyIcon != null)
                {
                    notifyIcon.BalloonTipTitle = "Monitor de Acessos";
                    notifyIcon.BalloonTipText = "Monitoramento iniciado.";
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon.ShowBalloonTip(3000);
                }
            }
            catch (Exception ex)
            {
                proxyIniciado = false;
                lblStatus.Text = "Status: erro ao iniciar proxy";
                AdicionarLog("ERRO ao iniciar proxy: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Erro ao iniciar proxy",
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

            btnIniciar.Enabled = true;
            btnParar.Enabled = false;
            lblStatus.Text = "Status: parado manualmente";

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
            if (InvokeRequired)
            {
                Invoke(new Action(() => MostrarNotificacao(acesso)));
                return;
            }

            FormNotificacaoAcesso notificacao = new FormNotificacaoAcesso(acesso);
            notificacao.Show();
        }

        private void AdicionarLog(string texto)
        {
            if (txtLog == null)
                return;

            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AdicionarLog(texto)));
                return;
            }

            txtLog.AppendText(texto + Environment.NewLine);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !encerrandoAplicacao)
            {
                e.Cancel = true;

                Hide();
                ShowInTaskbar = false;

                if (notifyIcon != null)
                {
                    notifyIcon.BalloonTipTitle = "Monitor de Acessos";
                    notifyIcon.BalloonTipText = "O monitor continuará rodando em segundo plano.";
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon.ShowBalloonTip(3000);
                }

                return;
            }

            try
            {
                if (timerVerificarControleAcesso != null)
                    timerVerificarControleAcesso.Stop();

                if (proxyService != null)
                    proxyService.Parar();

                ProxyWindowsService.ExecutarComandoDesativarProxy();
                ProxyWindowsService.DesativarProxyWindows();

                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
            }
            catch
            {
            }

            base.OnFormClosing(e);
        }
    }
}