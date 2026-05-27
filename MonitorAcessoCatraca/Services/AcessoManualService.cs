using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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

        public async Task<bool> LiberarAcessoManualAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token não informado para liberação manual.");

            string url =
                "https://" +
                AppConfig.HostAcesso +
                "/api/v1/ContratoClienteAcesso/AcessoManual";

            AcessoManualRequestDto body = new AcessoManualRequestDto
            {
                CodigoCliente = null,
                Motivo = "Via Monitor de Acesso"
            };

            string jsonEnvio = JsonConvert.SerializeObject(body);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(jsonEnvio, Encoding.UTF8, "application/json");

            request.Headers.TryAddWithoutValidation("User-Agent", "Controle de acesso - Next Fit - v1.10");
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
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

            ValidarRespostaApi(resposta);

            return true;
        }

        private void ValidarRespostaApi(string resposta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(resposta))
                    throw new Exception("API retornou resposta vazia.");

                JObject obj = JObject.Parse(resposta);

                bool success = obj["Success"] != null && obj["Success"].Value<bool>();

                if (!success)
                {
                    string mensagem = obj["Message"] != null
                        ? obj["Message"].ToString()
                        : "API retornou Success=false.";

                    throw new Exception("Liberação manual não confirmada pela API: " + mensagem);
                }

                JToken content = obj["Content"];

                if (content != null && content["AcessoLiberado"] != null)
                {
                    bool acessoLiberado = content["AcessoLiberado"].Value<bool>();

                    if (!acessoLiberado)
                    {
                        throw new Exception("API respondeu, mas AcessoLiberado veio false.");
                    }
                }
            }
            catch (JsonReaderException)
            {
                throw new Exception("Não foi possível interpretar a resposta da API: " + resposta);
            }
        }

        private string ObterCaminhoLog()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "debug_acesso_manual.txt"
            );
        }

        private string MascararToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return "";

            if (token.Length <= 20)
                return "***";

            return token.Substring(0, 10) + "...TOKEN_OCULTO..." + token.Substring(token.Length - 10);
        }
    }
}