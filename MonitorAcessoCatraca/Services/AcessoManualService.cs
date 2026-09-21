using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonitorAcessoCatraca.Services
{
    public class AcessoManualService
    {
        private readonly HttpClient _httpClient;

        public AcessoManualService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> LiberarAcessoManualAsync(string token, string codigoUnidade)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token não informado para liberação manual.");

            var equipamento = ObterEquipamentoAtivoBanco();

            if (!equipamento.HasValue)
            {
                throw new Exception("Nenhum equipamento ativo encontrado no banco.db3.");
            }

            int idEquipamento = equipamento.Value.Id;
            string descricaoEquipamento = string.IsNullOrWhiteSpace(equipamento.Value.Descricao)
                ? "Catraca"
                : equipamento.Value.Descricao;

            string url = "https://api.nextfit.com.br/api/ControleAcesso/LiberarEntrada";

            EquipamentoDto equipamentoDto = new EquipamentoDto
            {
                Id = idEquipamento,
                Descricao = descricaoEquipamento,
                Offline = false,
                Value = idEquipamento,
                Label = descricaoEquipamento
            };

            AcessoManualRequestDto body = new AcessoManualRequestDto
            {
                CodigoCliente = null,
                CodigoEquipamento = idEquipamento,
                TemEquipamentos = true,
                Equipamentos = new List<EquipamentoDto> { equipamentoDto },
                Equipamento = equipamentoDto
            };

            string jsonEnvio = JsonConvert.SerializeObject(body);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonEnvio, Encoding.UTF8, "application/json")
            };

            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
            if (!string.IsNullOrWhiteSpace(codigoUnidade))
            {
                request.Headers.TryAddWithoutValidation("codigo-unidade", codigoUnidade);
            }
            request.Headers.TryAddWithoutValidation("front-version", "1.1.5");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string resposta = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    "Erro ao liberar acesso manual. Status: " +
                    (int)response.StatusCode +
                    " - " +
                    resposta
                );
            }

            return true;
        }

        private (int Id, string Descricao)? ObterEquipamentoAtivoBanco()
        {
            try
            {
                string caminhoBanco = AppConfig.CAMINHO_BANCO;

                if (!File.Exists(caminhoBanco))
                    return null;

                using (var conn = new SQLiteConnection("Data Source=" + caminhoBanco + ";Version=3;Read Only=True;"))
                {
                    conn.Open();

                    // 1. Busca equipamento padrão e ativo
                    using (var cmd = new SQLiteCommand("SELECT IDEQUIPAMENTO, DESCRICAO FROM EQUIPAMENTO WHERE PADRAO = 1 AND INATIVO = 0 LIMIT 1", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (Convert.ToInt32(reader["IDEQUIPAMENTO"]), reader["DESCRICAO"]?.ToString());
                        }
                    }

                    // 2. Fallback: busca qualquer equipamento ativo
                    using (var cmd = new SQLiteCommand("SELECT IDEQUIPAMENTO, DESCRICAO FROM EQUIPAMENTO WHERE INATIVO = 0 LIMIT 1", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (Convert.ToInt32(reader["IDEQUIPAMENTO"]), reader["DESCRICAO"]?.ToString());
                        }
                    }

                    // 3. Fallback: primeiro equipamento cadastrado
                    using (var cmd = new SQLiteCommand("SELECT IDEQUIPAMENTO, DESCRICAO FROM EQUIPAMENTO LIMIT 1", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (Convert.ToInt32(reader["IDEQUIPAMENTO"]), reader["DESCRICAO"]?.ToString());
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}