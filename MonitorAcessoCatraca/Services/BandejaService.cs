using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Services
{
    public class BandejaService
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _menuBandeja;

        public event Action _abrirSolicitado;
        public event Action _iniciarSolicitado;
        public event Action _pararSolicitado;
        public event Action _sairSolicitado;

        public NotifyIcon NotifyIcon
        {
            get { return _notifyIcon; }
        }

        public void Configurar()
        {
            _menuBandeja = new ContextMenuStrip();

            ToolStripMenuItem abrirItem = new ToolStripMenuItem("Abrir monitor");
            abrirItem.Click += (s, e) => _abrirSolicitado?.Invoke();

            ToolStripMenuItem iniciarItem = new ToolStripMenuItem("Iniciar monitoramento");
            iniciarItem.Click += (s, e) => _iniciarSolicitado?.Invoke();

            ToolStripMenuItem pararItem = new ToolStripMenuItem("Parar monitoramento");
            pararItem.Click += (s, e) => _pararSolicitado?.Invoke();

            ToolStripMenuItem sairItem = new ToolStripMenuItem("Sair");
            sairItem.Click += (s, e) => _sairSolicitado?.Invoke();

            _menuBandeja.Items.Add(abrirItem);
            _menuBandeja.Items.Add(new ToolStripSeparator());
            _menuBandeja.Items.Add(iniciarItem);
            _menuBandeja.Items.Add(pararItem);
            _menuBandeja.Items.Add(new ToolStripSeparator());
            _menuBandeja.Items.Add(sairItem);

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = ObterIcone();
            _notifyIcon.Text = "Monitor de Acessos - Next Fit";
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = _menuBandeja;

            _notifyIcon.DoubleClick += (s, e) => _abrirSolicitado?.Invoke();
        }

        public void MostrarMensagem(string titulo, string texto, ToolTipIcon icone)
        {
            if (_notifyIcon == null)
                return;

            _notifyIcon.BalloonTipTitle = titulo;
            _notifyIcon.BalloonTipText = texto;
            _notifyIcon.BalloonTipIcon = icone;
            _notifyIcon.ShowBalloonTip(3000);
        }

        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            if (_menuBandeja != null)
            {
                _menuBandeja.Dispose();
                _menuBandeja = null;
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