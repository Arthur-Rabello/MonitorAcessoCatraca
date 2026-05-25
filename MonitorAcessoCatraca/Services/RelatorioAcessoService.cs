using MonitorAcessoCatraca.DTOs;
using MonitorAcessoCatraca.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MonitorAcessoCatraca.Services
{
    public class RelatorioAcessoService
    {
        private readonly HttpClient httpClient;

        public RelatorioAcessoService()
        {
            httpClient = new HttpClient();
        }

        public async Task<AcessoRelatorio> BuscarUltimoAcessoAsync(string token, int codigoUnidade)
        {
            DateTime dataFinal = DateTime.UtcNow.AddMinutes(2);
            DateTime dataInicial = DateTime.UtcNow.AddMinutes(-10);

            string fields = "[\"Id\",\"CodigoContratoClienteAcesso\",\"AcessoLiberado\",\"NomeCliente\",\"DescricaoContrato\",\"DataHora\",\"Motivo\",\"TipoMotivo\"]";
            string includes = "[]";
            string sort = "[{\"direction\":\"DESC\",\"property\":\"DataHora\"}]";
            string filter = "[]";

            string url =
                "https://api.nextfit.com.br/api/v2/RelCliente/RecuperarAcessosContrato" +
                "?limit=1" +
                "&page=1" +
                "&fields=" + Uri.EscapeDataString(fields) +
                "&includes=" + Uri.EscapeDataString(includes) +
                "&sort=" + Uri.EscapeDataString(sort) +
                "&DataFinal=" + Uri.EscapeDataString(dataFinal.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")) +
                "&DataInicial=" + Uri.EscapeDataString(dataInicial.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")) +
                "&AgruparAcessosPorCliente=false" +
                "&ExibirClientesAgregadores=true" +
                "&TipoAcesso=1" +
                "&filter=" + Uri.EscapeDataString(filter);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
            request.Headers.TryAddWithoutValidation("codigo-unidade", codigoUnidade.ToString());
            request.Headers.TryAddWithoutValidation("front-version", "1.1.5");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();


            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro ao consultar relatório. Status: " + (int)response.StatusCode + " - " + json);

            AcessoRelatorioDto dto = JsonConvert.DeserializeObject<AcessoRelatorioDto>(json);

            if (dto == null || dto.Content == null || dto.Content.Count == 0)
                return null;

            AcessoRelatorioItemDto item = dto.Content.FirstOrDefault();

            if (item == null)
                return null;

            return ConverterParaModel(item);
        }

        private AcessoRelatorio ConverterParaModel(AcessoRelatorioItemDto dto)
        {
            string motivo = ObterMotivoTratado(dto);

            return new AcessoRelatorio
            {
                Id = dto.Id,
                CodigoContratoClienteAcesso = dto.CodigoContratoClienteAcesso,
                NomeCliente = dto.NomeCliente,
                Contrato = dto.DescricaoContrato,
                DataHora = dto.DataHora.HasValue ? dto.DataHora.Value : DateTime.Now,
                Liberado = dto.AcessoLiberado,
                Motivo = motivo
            };
        }

        private string ObterMotivoTratado(AcessoRelatorioItemDto dto)
        {
            if (dto == null)
                return "Motivo não informado";

            if (!string.IsNullOrWhiteSpace(dto.Motivo) && !EhNumero(dto.Motivo))
                return dto.Motivo;

            if (!string.IsNullOrWhiteSpace(dto.TipoMotivo))
                return TraduzirTipoMotivo(dto.TipoMotivo);

            if (!string.IsNullOrWhiteSpace(dto.Motivo))
                return TraduzirTipoMotivo(dto.Motivo);

            if (dto.AcessoLiberado)
                return "Acesso autorizado";

            return "Acesso bloqueado";
        }

        private bool EhNumero(string valor)
        {
            int numero;
            return int.TryParse(valor, out numero);
        }

        private string TraduzirTipoMotivo(string tipoMotivo)
        {
            if (string.IsNullOrWhiteSpace(tipoMotivo))
                return "Motivo não informado";

            switch (tipoMotivo.Trim())
            {
                case "0":
                    return "Manual";

                case "1":
                    return "Cliente sem contrato ativo";

                case "2":
                    return "Contrato sem modalidade";

                case "3":
                    return "Contrato sem sessão disponível";

                case "4":
                    return "Contrato sem aula no dia";

                case "5":
                    return "Contrato fora do dia permitido";

                case "6":
                    return "Contrato fora do horário permitido";

                case "7":
                    return "Contrato quantidade de acessos na semana atingido";

                case "8":
                    return "Contrato quantidade de acessos no período atingido";

                default:
                    return "Motivo não identificado: " + tipoMotivo;
            }
        }

    }
}