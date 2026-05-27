using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MonitorAcessoCatraca
{
    public class NextFitApiClient
    {
        private readonly string _caminhoBanco;
        private readonly HttpClient _http;

        private string _token;
        private string _codigoUnidade;

        public NextFitApiClient(string caminhoBanco)
        {
            _caminhoBanco = caminhoBanco;

            _http = new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<bool> LoginAsync()
        {
            var config = LerConfiguracaoBanco();

            if (config == null)
            {
                throw new Exception("Não foi possível ler EMAIL e SENHA da tabela CONFIGURACAO.");
            }

            var url = "https://api.nextfit.com.br/api/token/";

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", config.Email),
                new KeyValuePair<string, string>("password", config.Senha),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = form;
            request.Headers.Add("Front-Version", "1.1.5");

            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Erro ao realizar login na API: " + body);
            }

            var json = JObject.Parse(body);

            _token = json["access_token"]?.ToString();

            if (string.IsNullOrWhiteSpace(_token))
            {
                throw new Exception("A API não retornou access_token.");
            }

            _codigoUnidade = ExtrairCodigoUnidadeDoToken(_token);

            if (string.IsNullOrWhiteSpace(_codigoUnidade))
            {
                _codigoUnidade = "1";
            }

            ConfigurarHeadersPadrao();

            return true;
        }

        public async Task AceitarTermosAsync()
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                return;
            }

            var url = "https://api.nextfit.com.br/api/UsuarioTermosUso/InserirDTO";

            var jsonVazio = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, jsonVazio);
        }

        public async Task<List<AcessoRelatorio>> BuscarAcessosAsync(DateTime dataInicialLocal, DateTime dataFinalLocal, int pagina = 1, int limite = 30)
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                await LoginAsync();
            }

            var dataInicialBrasil = new DateTimeOffset(dataInicialLocal, TimeSpan.FromHours(-3));
            var dataFinalBrasil = new DateTimeOffset(dataFinalLocal, TimeSpan.FromHours(-3));

            string dataInicialUtc = dataInicialBrasil.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            string dataFinalUtc = dataFinalBrasil.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

            var fields = "[\"Id\",\"Descricao\",\"Inativo\",\"AcessoLiberado\",\"NomeCliente\",\"DescricaoContrato\",\"DataHora\",\"ModoReconhecimento\",\"Offline\",\"Motivo\",\"TipoMotivo\"]";
            var includes = "[]";
            var sort = "[{\"direction\":\"DESC\",\"property\":\"Id\"}]";
            var filter = "[]";

            var url =
                "https://api.nextfit.com.br/api/v2/RelCliente/RecuperarAcessosContrato" +
                "?limit=" + limite +
                "&page=" + pagina +
                "&fields=" + Uri.EscapeDataString(fields) +
                "&includes=" + Uri.EscapeDataString(includes) +
                "&sort=" + Uri.EscapeDataString(sort) +
                "&DataFinal=" + Uri.EscapeDataString(dataFinalUtc) +
                "&DataInicial=" + Uri.EscapeDataString(dataInicialUtc) +
                "&AgruparAcessosPorCliente=false" +
                "&ExibirClientesAgregadores=true" +
                "&TipoAcesso=1" +
                "&filter=" + Uri.EscapeDataString(filter);

            System.IO.File.WriteAllText(
                "debug_requisicao_url.txt",
                url
            );

            System.IO.File.WriteAllText(
                "debug_requisicao_headers.txt",
                "Authorization: Bearer " + (_token != null ? _token.Substring(0, Math.Min(20, _token.Length)) + "..." : "SEM TOKEN") + Environment.NewLine +
                "Codigo-Unidade: " + _codigoUnidade + Environment.NewLine +
                "Front-Version: 1.1.5" + Environment.NewLine +
                "Origin: https://app.nextfit.com.br" + Environment.NewLine +
                "Referer: https://app.nextfit.com.br/"
            );

            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            System.IO.File.WriteAllText(
                "debug_resposta_api.txt",
                "STATUS: " + ((int)response.StatusCode).ToString() + " - " + response.StatusCode.ToString() +
                Environment.NewLine +
                Environment.NewLine +
                body
            );

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await LoginAsync();
                response = await _http.GetAsync(url);
                body = await response.Content.ReadAsStringAsync();
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Erro ao consultar relatório de acessos: " + body);
            }

            return ParseAcessos(body);
        }

        private void ConfigurarHeadersPadrao()
        {
            _http.DefaultRequestHeaders.Clear();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            _http.DefaultRequestHeaders.Add("Codigo-Unidade", _codigoUnidade);
            _http.DefaultRequestHeaders.Add("Front-Version", "1.1.5");
            _http.DefaultRequestHeaders.Add("Origin", "https://app.nextfit.com.br");
            _http.DefaultRequestHeaders.Add("Referer", "https://app.nextfit.com.br/");
            _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private List<AcessoRelatorio> ParseAcessos(string json)
        {
            var resultado = new List<AcessoRelatorio>();

            var root = JObject.Parse(json);

            JToken lista = root["Content"];

            if (lista == null)
            {
                lista = root["content"];
            }

            if (lista == null || lista.Type != JTokenType.Array)
            {
                return resultado;
            }

            foreach (var item in lista)
            {
                int codigoAcesso = LerInt(item, "CodigoContratoClienteAcesso");

                if (codigoAcesso == 0)
                {
                    codigoAcesso = LerInt(item, "Id");
                }

                var acesso = new AcessoRelatorio();

                acesso.Id = codigoAcesso;
                acesso.CodigoContratoClienteAcesso = codigoAcesso;

                acesso.CodigoCliente = LerInt(item, "CodigoCliente");
                acesso.CodigoUsuario = LerInt(item, "CodigoUsuario");
                acesso.CodigoContratoCliente = LerInt(item, "CodigoContratoCliente");

                acesso.Descricao = LerString(item, "Descricao");
                acesso.Inativo = LerBool(item, "Inativo");
                acesso.AcessoLiberado = LerBool(item, "AcessoLiberado");
                acesso.NomeCliente = LerString(item, "NomeCliente");
                acesso.DescricaoContrato = LerString(item, "DescricaoContrato");

                acesso.ModoReconhecimento = LerString(item, "ModoReconhecimento");
                acesso.Offline = LerBool(item, "Offline");
                acesso.Motivo = LerString(item, "Motivo");

                acesso.TipoMotivo = LerIntNullable(item, "TipoMotivo");
                acesso.Tipo = LerIntNullable(item, "Tipo");

                string dataStr = LerString(item, "DataHora");

                if (!string.IsNullOrWhiteSpace(dataStr))
                {
                    DateTime data;

                    if (DateTime.TryParse(dataStr, out data))
                    {
                        acesso.DataHora = data.ToLocalTime();
                    }
                }

                resultado.Add(acesso);
            }

            return resultado;
        }

        private int LerInt(JToken item, string nomeCampo)
        {
            try
            {
                var token = item[nomeCampo];

                if (token == null || token.Type == JTokenType.Null)
                {
                    return 0;
                }

                int valor;

                if (int.TryParse(token.ToString(), out valor))
                {
                    return valor;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private int? LerIntNullable(JToken item, string nomeCampo)
        {
            try
            {
                var token = item[nomeCampo];

                if (token == null || token.Type == JTokenType.Null)
                {
                    return null;
                }

                int valor;

                if (int.TryParse(token.ToString(), out valor))
                {
                    return valor;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool LerBool(JToken item, string nomeCampo)
        {
            try
            {
                var token = item[nomeCampo];

                if (token == null || token.Type == JTokenType.Null)
                {
                    return false;
                }

                bool valor;

                if (bool.TryParse(token.ToString(), out valor))
                {
                    return valor;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string LerString(JToken item, string nomeCampo)
        {
            try
            {
                var token = item[nomeCampo];

                if (token == null || token.Type == JTokenType.Null)
                {
                    return "";
                }

                return token.ToString();
            }
            catch
            {
                return "";
            }
        }

        private ConfigApi LerConfiguracaoBanco()
        {
            using (var conn = new SQLiteConnection("Data Source=" + _caminhoBanco + ";Version=3;"))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand("SELECT EMAIL, SENHA FROM CONFIGURACAO LIMIT 1", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ConfigApi
                        {
                            Email = reader["EMAIL"]?.ToString(),
                            Senha = reader["SENHA"]?.ToString()
                        };
                    }
                }
            }

            return null;
        }

        private string ExtrairCodigoUnidadeDoToken(string token)
        {
            try
            {
                var partes = token.Split('.');

                if (partes.Length < 2)
                {
                    return null;
                }

                string payload = partes[1];

                payload = payload.Replace('-', '+').Replace('_', '/');

                switch (payload.Length % 4)
                {
                    case 2:
                        payload += "==";
                        break;
                    case 3:
                        payload += "=";
                        break;
                }

                var bytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(bytes);

                var obj = JObject.Parse(json);

                var unidade = obj["codigoUnidadePreferencial"]?.ToString();

                if (string.IsNullOrWhiteSpace(unidade))
                {
                    unidade = obj["codigoTenant"]?.ToString();
                }

                return unidade;
            }
            catch
            {
                return null;
            }
        }

        private class ConfigApi
        {
            public string Email { get; set; }
            public string Senha { get; set; }
        }
    }
}