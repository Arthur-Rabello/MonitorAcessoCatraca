using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.DTOs;
using System;
using System.Data.SQLite;
using System.IO;

namespace MonitorAcessoCatraca.Services
{
    public class ConfiguracaoService
    {
        public ConfiguracaoApiDto ObterConfiguracao()
        {
            if (!File.Exists(AppConfig.CaminhoBanco))
                throw new Exception("Banco de dados não encontrado em: " + AppConfig.CaminhoBanco);

            string connectionString = "Data Source=" + AppConfig.CaminhoBanco + ";Version=3;";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT EMAIL, SENHA
                    FROM CONFIGURACAO
                    LIMIT 1;
                ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string email = reader["EMAIL"] == DBNull.Value ? "" : reader["EMAIL"].ToString();
                        string senha = reader["SENHA"] == DBNull.Value ? "" : reader["SENHA"].ToString();

                        return new ConfiguracaoApiDto
                        {
                            Email = email,
                            Senha = senha
                        };
                    }
                }
            }

            throw new Exception("Não foi possível encontrar EMAIL e SENHA na tabela CONFIGURACAO.");
        }
    }
}