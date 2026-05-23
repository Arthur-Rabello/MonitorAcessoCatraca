using MonitorAcessoCatraca.Forms;
using MonitorAcessoCatraca.Services;
using System;
using System.Windows.Forms;

namespace MonitorAcessoCatraca
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ProxyWindowsService.DesativarProxyWindows();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ApplicationExit += Application_ApplicationExit;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                Application.Run(new FormPrincipal());
            }
            finally
            {
                ProxyWindowsService.DesativarProxyWindows();
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
    }
}