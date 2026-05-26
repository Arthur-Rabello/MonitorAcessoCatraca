using MonitorAcessoCatraca.Services;
using System;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Forms
{
    public class FormPrincipal : Form
    {
        private MonitorAcessoService monitorAcessoService;

        public FormPrincipal()
        {
            monitorAcessoService = new MonitorAcessoService(this);
            monitorAcessoService.Inicializar();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            monitorAcessoService.AoExibirTela();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            monitorAcessoService.AoCriarHandle();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            monitorAcessoService.AoDestruirHandle();
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (monitorAcessoService != null && monitorAcessoService.ProcessarMensagemWindows(m))
                return;

            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            monitorAcessoService.AoFecharFormulario(e);

            if (!e.Cancel)
                base.OnFormClosing(e);
        }
    }
}