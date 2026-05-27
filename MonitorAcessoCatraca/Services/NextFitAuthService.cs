using MonitorAcessoCatraca.DTOs;
using MonitorAcessoCatraca.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MonitorAcessoCatraca.Services
{
    public class NextFitAuthService
    {
        private readonly HttpClient _http;

        public string Token { get; private set; }
        public string CodigoUnidade { get; private set; }

        public NextFitAuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task LoginAsync(string email, string senha)
        {
            var url = "https://api.nextfit.com.br/api/token/";

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", email),
                new KeyValuePair<string, string>("password", senha),
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

            var login = JsonConvert.DeserializeObject<LoginResponseDto>(body);

            if (login == null || string.IsNullOrWhiteSpace(login.AccessToken))
            {
                throw new Exception("A API não retornou access_token.");
            }

            Token = login.AccessToken;
            CodigoUnidade = JwtHelper.ExtrairCodigoUnidade(Token);

            if (string.IsNullOrWhiteSpace(CodigoUnidade))
            {
                CodigoUnidade = "1";
            }

            ConfigurarHeadersPadrao();
        }

        public async Task AceitarTermosAsync()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                return;
            }

            var url = "https://api.nextfit.com.br/api/UsuarioTermosUso/InserirDTO";
            var jsonVazio = new StringContent("{}", Encoding.UTF8, "application/json");

            try
            {
                await _http.PostAsync(url, jsonVazio);
            }
            catch
            {
            }
        }

        private void ConfigurarHeadersPadrao()
        {
            _http.DefaultRequestHeaders.Clear();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            _http.DefaultRequestHeaders.Add("Codigo-Unidade", CodigoUnidade);
            _http.DefaultRequestHeaders.Add("Front-Version", "1.1.5");
            _http.DefaultRequestHeaders.Add("Origin", "https://app.nextfit.com.br");
            _http.DefaultRequestHeaders.Add("Referer", "https://app.nextfit.com.br/");
            _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}