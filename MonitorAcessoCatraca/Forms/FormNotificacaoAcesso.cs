using MonitorAcessoCatraca.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MonitorAcessoCatraca.Forms
{
    public class FormNotificacaoAcesso : Form
    {
        private Timer _timerFechar;

        public FormNotificacaoAcesso(AcessoAutomatico acesso)
        {
            MontarTela(acesso);
        }

        private void MontarTela(AcessoAutomatico acesso)
        {
            bool liberado = acesso.Liberado;

            Width = 390;
            Height = 180;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = liberado ? Color.FromArgb(35, 150, 80) : Color.FromArgb(190, 55, 55);

            Rectangle areaTrabalho = Screen.PrimaryScreen.WorkingArea;

            Left = areaTrabalho.Right - Width - 20;
            Top = areaTrabalho.Bottom - Height - 45;

            var lblTitulo = new Label
            {
                Left = 20,
                Top = 15,
                Width = 340,
                Height = 30,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Text = liberado ? "✅ ACESSO LIBERADO" : "⛔ ACESSO BLOQUEADO"
            };

            Controls.Add(lblTitulo);

            var lblCliente = new Label
            {
                Left = 20,
                Top = 50,
                Width = 340,
                Height = 25,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = string.IsNullOrWhiteSpace(acesso.NomeCliente)
                    ? "Cliente não identificado"
                    : acesso.NomeCliente
            };

            Controls.Add(lblCliente);

            var lblHorario = new Label
            {
                Left = 20,
                Top = 78,
                Width = 340,
                Height = 22,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Text = "Horário: " + acesso.DataHora.ToString("dd/MM/yyyy HH:mm:ss")
            };

            Controls.Add(lblHorario);

            var lblServico = new Label
            {
                Left = 20,
                Top = 101,
                Width = 340,
                Height = 22,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Text = "Serviço: " + acesso.ServicoTratado
            };

            Controls.Add(lblServico);

            var lblMotivo = new Label
            {
                Left = 20,
                Top = 124,
                Width = 340,
                Height = 42,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Text = "Motivo: " + acesso.MotivoTratado
            };

            Controls.Add(lblMotivo);

            var btnFechar = new Button
            {
                Text = "X",
                Left = Width - 38,
                Top = 8,
                Width = 28,
                Height = 25,
                FlatStyle = FlatStyle.Flat,
                BackColor = BackColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            btnFechar.FlatAppearance.BorderSize = 0;
            btnFechar.Click += (s, e) => Close();

            Controls.Add(btnFechar);

            _timerFechar = new Timer();
            _timerFechar.Interval = 7000;
            _timerFechar.Tick += TimerFechar_Tick;
            _timerFechar.Start();
        }

        private void TimerFechar_Tick(object sender, EventArgs e)
        {
            _timerFechar.Stop();
            Close();
        }
    }
}