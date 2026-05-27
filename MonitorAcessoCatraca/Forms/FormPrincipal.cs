using MonitorAcessoCatraca.Services;
using System;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Forms
{
    public class FormPrincipal : Form
    {
        private MonitorAcessoService _monitorAcessoService;

        public FormPrincipal()
        {
            _monitorAcessoService = new MonitorAcessoService(this);
            _monitorAcessoService.Inicializar();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _monitorAcessoService.AoExibirTela();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _monitorAcessoService.AoCriarHandle();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _monitorAcessoService.AoDestruirHandle();
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (_monitorAcessoService != null && _monitorAcessoService.ProcessarMensagemWindows(m))
                return;

            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _monitorAcessoService.AoFecharFormulario(e);

            if (!e.Cancel)
                base.OnFormClosing(e);
        }
    }
}