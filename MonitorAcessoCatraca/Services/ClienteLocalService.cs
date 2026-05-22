using MonitorAcessoCatraca.Config;
using System;
using System.Data.SQLite;
using System.IO;

namespace MonitorAcessoCatraca.Services
{
	public class ClienteLocalService
	{
		public string BuscarNomeCliente(int codigoCliente)
		{
			try
			{
				if (codigoCliente <= 0)
					return "";

				if (!File.Exists(AppConfig.CaminhoBanco))
					return "";

				string connectionString = "Data Source=" + AppConfig.CaminhoBanco + ";Version=3;";

				using (SQLiteConnection conn = new SQLiteConnection(connectionString))
				{
					conn.Open();

					string sql = @"
                        SELECT NOME
                        FROM CLIENTES
                        WHERE IDCLIENTE = @IDCLIENTE
                        LIMIT 1;
                    ";

					using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
					{
						cmd.Parameters.AddWithValue("@IDCLIENTE", codigoCliente);

						object result = cmd.ExecuteScalar();

						if (result == null || result == DBNull.Value)
							return "";

						return result.ToString();
					}
				}
			}
			catch
			{
				return "";
			}
		}
	}
}