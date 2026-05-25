using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Services
{
    public static class InicializacaoWindowsService
    {
        private const string NomeAplicativo = "MonitorAcessoCatraca";

        public static void AtivarInicializacaoComWindows()
        {
            try
            {
                string caminhoExe = Application.ExecutablePath;

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run",
                    true
                ))
                {
                    if (key == null)
                        return;

                    key.SetValue(NomeAplicativo, "\"" + caminhoExe + "\"");
                }
            }
            catch
            {
            }
        }

        public static void DesativarInicializacaoComWindows()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run",
                    true
                ))
                {
                    if (key == null)
                        return;

                    key.DeleteValue(NomeAplicativo, false);
                }
            }
            catch
            {
            }
        }

        public static bool EstaAtivado()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run",
                    false
                ))
                {
                    if (key == null)
                        return false;

                    object valor = key.GetValue(NomeAplicativo);

                    return valor != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}