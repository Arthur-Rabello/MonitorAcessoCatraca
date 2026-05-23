using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.Models;
using MonitorAcessoCatraca.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

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

        private ProcessoService processoService;
        private ClienteLocalService clienteLocalService;
        private AcessoAutomaticoParserService parserService;
        private ProxyInterceptacaoService proxyService;

        private bool proxyIniciado = false;
        private bool paradoManualmente = false;

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
                Text = "Interceptando: " + AppConfig.HostAcesso + AppConfig.EndpointAcessoAutomatico,
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
            clienteLocalService = new ClienteLocalService();
            parserService = new AcessoAutomaticoParserService(clienteLocalService);
            proxyService = new ProxyInterceptacaoService(parserService);
            proxyService.AcessoCapturado += ProxyService_AcessoCapturado;

            timerVerificarControleAcesso = new Timer();
            timerVerificarControleAcesso.Interval = 5000;
            timerVerificarControleAcesso.Tick += TimerVerificarControleAcesso_Tick;
            timerVerificarControleAcesso.Start();

            ConfigurarBandeja();

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
            notifyIcon.Icon = SystemIcons.Application;
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
                lblStatus.Text = "Status: Controle de Acesso não está aberto. Tentando abrir...";

                string mensagem;
                bool abriu = processoService.TentarAbrirControleAcesso(out mensagem);

                AdicionarLog(mensagem);

                if (!abriu)
                {
                    lblStatus.Text = "Status: aguardando Controle de Acesso...";
                    return;
                }

                lblStatus.Text = "Status: aguardando inicialização do Controle de Acesso...";
                return;
            }

            if (!proxyIniciado)
            {
                AdicionarLog("Controle de Acesso detectado. Iniciando interceptação...");
                IniciarProxy();
            }
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

        private void ProxyService_AcessoCapturado(AcessoAutomatico acesso)
        {
            if (acesso == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ProxyService_AcessoCapturado(acesso)));
                return;
            }

            AdicionarLog(FormatarAcesso(acesso));
            MostrarNotificacao(acesso);
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
            try
            {
                if (timerVerificarControleAcesso != null)
                    timerVerificarControleAcesso.Stop();

                if (proxyService != null)
                    proxyService.Parar();

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