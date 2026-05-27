using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Services
{
    public class HotkeyGlobalService
    {
        private const int WM_HOTKEY = 0x0312;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_NOREPEAT = 0x4000;

        private readonly int _id;
        private readonly uint _modificadores;
        private readonly Keys _tecla;

        private bool registrado = false;

        public event Action AtalhoPressionado;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotkeyGlobalService(int id, Keys tecla, bool ctrl, bool alt, bool shift)
        {
            this._id = id;
            this._tecla = tecla;

            _modificadores = MOD_NOREPEAT;

            if (ctrl)
            {
                _modificadores |= MOD_CONTROL;
            }
            if (alt)
            {
                _modificadores |= MOD_ALT;
            }
            if (shift)
            {
                _modificadores |= MOD_SHIFT;
            }
        }

        public bool Registrar(IntPtr handle, out int erroWindows)
        {
            erroWindows = 0;

            if (registrado)
            {
                return true;
            }

            bool sucesso = RegisterHotKey(
                handle,
                _id,
                _modificadores,
                (uint)_tecla
            );

            if (sucesso)
            {
                registrado = true;
                return true;
            }

            erroWindows = Marshal.GetLastWin32Error();
            return false;
        }

        public void Desregistrar(IntPtr handle)
        {
            if (!registrado)
            {
                return;
            }
            try
            {
                UnregisterHotKey(handle, _id);
            }
            catch
            {
            }

            registrado = false;
        }

        public bool ProcessarMensagem(Message mensagem)
        {
            if (mensagem.Msg != WM_HOTKEY)
            {
                return false;
            }

            int idMensagem = mensagem.WParam.ToInt32();

            if (idMensagem != _id)
            {
                return false;
            }
            AtalhoPressionado?.Invoke();
            return true;
        }

        public string ObterTextoAtalho()
        {
            string texto = "";

            if ((_modificadores & MOD_CONTROL) == MOD_CONTROL)
            {
                texto += "Ctrl + ";
            }
            if ((_modificadores & MOD_ALT) == MOD_ALT)
            {
                texto += "Alt + ";
            }
            if ((_modificadores & MOD_SHIFT) == MOD_SHIFT)
            {
                texto += "Shift + ";
            }
            texto += _tecla.ToString();

            return texto;
        }
    }
}