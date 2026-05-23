using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonitorAcessoCatraca.Services
{
    public static class ProxyWindowsService
    {
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(
            IntPtr hInternet,
            int dwOption,
            IntPtr lpBuffer,
            int dwBufferLength
        );

        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;

        public static void DesativarProxyWindows()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                    true
                ))
                {
                    if (key == null)
                        return;

                    key.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                    key.DeleteValue("ProxyServer", false);
                    key.DeleteValue("AutoConfigURL", false);
                }

                ExecutarComandoDesativarProxy();
                AtualizarConfiguracaoInternet();
            }
            catch
            {
                ExecutarComandoDesativarProxy();
            }
        }

        public static void DesativarProxyDoMonitor()
        {
            DesativarProxyWindows();
        }

        public static void ForcarDesativarProxy()
        {
            DesativarProxyWindows();
        }

        public static void ExecutarComandoDesativarProxy()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c REG ADD \"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\" /v ProxyEnable /t REG_DWORD /d 0x00000000 /f",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process processo = Process.Start(startInfo))
                {
                    if (processo != null)
                        processo.WaitForExit();
                }
            }
            catch
            {
            }
        }

        private static void AtualizarConfiguracaoInternet()
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
    }
}