using System.Windows.Forms;

namespace MonitorAcessoCatraca.Services
{
	public class FormPrincipalControles
	{
		public Button BtnIniciar { get; set; }
		public Button BtnParar { get; set; }
		public TextBox TxtLog { get; set; }
		public Label LblStatus { get; set; }
		public Label LblInfo { get; set; }
		public Timer TimerVerificarControleAcesso { get; set; }
	}
}