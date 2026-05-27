using MonitorAcessoCatraca.Forms;
using MonitorAcessoCatraca.Services;
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
                ProxyWindowsService.DesativarProxyWindows();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.ApplicationExit += Application_ApplicationExit;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Application.Run(new FormPrincipal());
            }
            finally
            {
                FinalizarAplicacao();
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            ProxyWindowsService.DesativarProxyWindows();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ProxyWindowsService.DesativarProxyWindows();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ProxyWindowsService.DesativarProxyWindows();
        }

        private static void FinalizarAplicacao()
        {
            try
            {
                ProxyWindowsService.ExecutarComandoDesativarProxy();
                ProxyWindowsService.DesativarProxyWindows();
            }
            catch
            {
            }

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