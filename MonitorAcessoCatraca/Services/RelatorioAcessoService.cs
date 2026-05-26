using MonitorAcessoCatraca.DTOs;
using MonitorAcessoCatraca.Enums;
using MonitorAcessoCatraca.Models;
using MonitorAcessoCatraca.Utils;
using Newtonsoft.Json;
using System;
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

            if (dto == null)
                return null;

            AcessoRelatorioItemDto item = dto.ObterPrimeiroContent();

            if (item == null)
                return null;

            return ConverterParaModel(item);
        }

        public AcessoRelatorio ConverterParaModel(AcessoRelatorioItemDto dto)
        {
            if (dto == null)
                return null;

            string motivo = ObterMotivoTratado(dto);

            return new AcessoRelatorio
            {
                Id = dto.Id,
                CodigoContratoClienteAcesso = dto.CodigoContratoClienteAcesso,
                NomeCliente = string.IsNullOrWhiteSpace(dto.NomeCliente)
                    ? ObterNomeClientePadrao(dto)
                    : dto.NomeCliente,
                Contrato = string.IsNullOrWhiteSpace(dto.DescricaoContrato)
                    ? ObterContratoPadrao(dto)
                    : dto.DescricaoContrato,
                DataHora = dto.DataHora.HasValue ? dto.DataHora.Value : DateTime.Now,
                Liberado = dto.AcessoLiberado,
                Motivo = motivo
            };
        }

        public AcessoAutomatico ConverterParaAcessoAutomaticoManual(AcessoRelatorioItemDto dto)
        {
            if (dto == null)
                throw new Exception("API não retornou Content no acesso manual.");

            if (!ValidarAcessoManual(dto))
                throw new Exception("Resposta de acesso manual inválida. AcessoLiberado: " + dto.AcessoLiberado + " | TipoMotivo: " + dto.TipoMotivo);

            return new AcessoAutomatico
            {
                CodigoCliente = dto.CodigoCliente,
                NomeCliente = string.IsNullOrWhiteSpace(dto.NomeCliente)
                    ? "Liberação manual"
                    : dto.NomeCliente,
                Liberado = true,
                Servico = string.IsNullOrWhiteSpace(dto.DescricaoContrato)
                    ? "Acesso manual"
                    : dto.DescricaoContrato,
                DataHora = dto.DataHora.HasValue
                    ? dto.DataHora.Value
                    : DateTime.Now,
                Motivo = string.IsNullOrWhiteSpace(dto.Motivo)
                    ? "Via Monitor de Acesso"
                    : dto.Motivo,
                Mensagem = "Acesso manual liberado"
            };
        }

        public bool ValidarAcessoManual(AcessoRelatorioItemDto dto)
        {
            if (dto == null)
                return false;

            if (!dto.AcessoLiberado)
                return false;

            if (!dto.TipoMotivo.HasValue)
                return false;

            return dto.TipoMotivo.Value == 0;
        }

        private string ObterMotivoTratado(AcessoRelatorioItemDto dto)
        {
            if (dto == null)
                return "Motivo não informado";

            if (!string.IsNullOrWhiteSpace(dto.Motivo))
                return dto.Motivo;

            if (EhAcessoManual(dto))
                return "Via Monitor de Acesso";

            if (dto.TipoMotivo.HasValue)
                return TraduzirTipoMotivo(dto.TipoMotivo);

            if (dto.AcessoLiberado)
                return "Acesso autorizado";

            return "Acesso bloqueado";
        }

        private bool EhAcessoManual(AcessoRelatorioItemDto dto)
        {
            return dto != null &&
                   dto.AcessoLiberado &&
                   dto.TipoMotivo.HasValue &&
                   dto.TipoMotivo.Value == 0;
        }

        private string ObterNomeClientePadrao(AcessoRelatorioItemDto dto)
        {
            if (EhAcessoManual(dto))
                return "Liberação manual";

            return "Cliente não informado";
        }

        private string ObterContratoPadrao(AcessoRelatorioItemDto dto)
        {
            if (EhAcessoManual(dto))
                return "Acesso manual";

            return "Contrato não informado";
        }

        private string TraduzirTipoMotivo(int? tipoMotivo)
        {
            if (!tipoMotivo.HasValue)
                return "Motivo não informado";

            if (!Enum.IsDefined(typeof(TipoMotivoAcessoEnum), tipoMotivo.Value))
                return "Motivo não identificado: " + tipoMotivo.Value;

            TipoMotivoAcessoEnum motivoEnum = (TipoMotivoAcessoEnum)tipoMotivo.Value;

            return EnumHelper.ObterDescricao(motivoEnum);
        }
    }
}