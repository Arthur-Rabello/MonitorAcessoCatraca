using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Services
{
    public class BandejaService
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip menuBandeja;

        public event Action AbrirSolicitado;
        public event Action IniciarSolicitado;
        public event Action PararSolicitado;
        public event Action SairSolicitado;

        public NotifyIcon NotifyIcon
        {
            get { return notifyIcon; }
        }

        public void Configurar()
        {
            menuBandeja = new ContextMenuStrip();

            ToolStripMenuItem abrirItem = new ToolStripMenuItem("Abrir monitor");
            abrirItem.Click += (s, e) => AbrirSolicitado?.Invoke();

            ToolStripMenuItem iniciarItem = new ToolStripMenuItem("Iniciar monitoramento");
            iniciarItem.Click += (s, e) => IniciarSolicitado?.Invoke();

            ToolStripMenuItem pararItem = new ToolStripMenuItem("Parar monitoramento");
            pararItem.Click += (s, e) => PararSolicitado?.Invoke();

            ToolStripMenuItem sairItem = new ToolStripMenuItem("Sair");
            sairItem.Click += (s, e) => SairSolicitado?.Invoke();

            menuBandeja.Items.Add(abrirItem);
            menuBandeja.Items.Add(new ToolStripSeparator());
            menuBandeja.Items.Add(iniciarItem);
            menuBandeja.Items.Add(pararItem);
            menuBandeja.Items.Add(new ToolStripSeparator());
            menuBandeja.Items.Add(sairItem);

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = ObterIcone();
            notifyIcon.Text = "Monitor de Acessos - Next Fit";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = menuBandeja;

            notifyIcon.DoubleClick += (s, e) => AbrirSolicitado?.Invoke();
        }

        public void MostrarMensagem(string titulo, string texto, ToolTipIcon icone)
        {
            if (notifyIcon == null)
                return;

            notifyIcon.BalloonTipTitle = titulo;
            notifyIcon.BalloonTipText = texto;
            notifyIcon.BalloonTipIcon = icone;
            notifyIcon.ShowBalloonTip(3000);
        }

        public void Dispose()
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }

            if (menuBandeja != null)
            {
                menuBandeja.Dispose();
                menuBandeja = null;
            }
        }

        private Icon ObterIcone()
        {
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
                return new Icon(caminhoIcone);

            return SystemIcons.Application;
        }
    }
}