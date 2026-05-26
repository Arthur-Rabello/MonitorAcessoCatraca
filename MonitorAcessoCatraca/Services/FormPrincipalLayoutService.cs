using MonitorAcessoCatraca.Config;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Services
{
    public class FormPrincipalLayoutService
    {
        public FormPrincipalControles Montar(Form form)
        {
            form.Text = "Monitor de Acessos - Next Fit";
            form.Width = 850;
            form.Height = 560;
            form.StartPosition = FormStartPosition.CenterScreen;

            FormPrincipalControles controles = new FormPrincipalControles();

            controles.LblInfo = new Label
            {
                Text = "Monitorando: " + AppConfig.HostAcesso + " / " + AppConfig.EndpointAcessoAutomatico,
                Left = 20,
                Top = 25,
                Width = 790,
                Height = 25
            };

            form.Controls.Add(controles.LblInfo);

            controles.BtnIniciar = new Button
            {
                Text = "Iniciar monitoramento",
                Left = 20,
                Top = 65,
                Width = 180,
                Height = 35
            };

            form.Controls.Add(controles.BtnIniciar);

            controles.BtnParar = new Button
            {
                Text = "Parar",
                Left = 210,
                Top = 65,
                Width = 100,
                Height = 35,
                Enabled = false
            };

            form.Controls.Add(controles.BtnParar);

            controles.LblStatus = new Label
            {
                Text = "Status: parado",
                Left = 330,
                Top = 75,
                Width = 470,
                Height = 25
            };

            form.Controls.Add(controles.LblStatus);

            controles.TxtLog = new TextBox
            {
                Left = 20,
                Top = 120,
                Width = 790,
                Height = 370,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            form.Controls.Add(controles.TxtLog);

            controles.TimerVerificarControleAcesso = new Timer
            {
                Interval = 5000
            };

            return controles;
        }
    }
}