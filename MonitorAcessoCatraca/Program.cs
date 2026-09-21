using MonitorAcessoCatraca.Forms;
using System;
using System.Threading;
using System.Windows.Forms;

namespace MonitorAcessoCatraca
{
    internal static class Program
    {
        private static Mutex _mutex;

        [STAThread]
        static void Main()
        {
            bool criouNovaInstancia;

            _mutex = new Mutex(
                true,
                "Global\\MonitorAcessoCatraca_NextFit_Unico",
                out criouNovaInstancia
            );

            if (!criouNovaInstancia)
            {
                MessageBox.Show(
                    "O Monitor de Acesso já está em execução.",
                    "Monitor de Acesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new FormPrincipal());
            }
            finally
            {
                FinalizarAplicacao();
            }
        }

        private static void FinalizarAplicacao()
        {
            try
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                    _mutex.Dispose();
                    _mutex = null;
                }
            }
            catch
            {
            }
        }
    }
}