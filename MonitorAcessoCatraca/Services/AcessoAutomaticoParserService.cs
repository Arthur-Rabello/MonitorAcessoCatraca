using MonitorAcessoCatraca.DTOs;
using MonitorAcessoCatraca.Models;
using Newtonsoft.Json;
using System;

namespace MonitorAcessoCatraca.Services
{
    public class AcessoAutomaticoParserService
    {
        private readonly ClienteLocalService _clienteLocalService;

        public AcessoAutomaticoParserService(ClienteLocalService clienteLocalService)
        {
            _clienteLocalService = clienteLocalService;
        }

        public AcessoAutomatico ConverterJsonParaAcesso(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            AcessoAutomaticoResponseDto response;

            try
            {
                response = JsonConvert.DeserializeObject<AcessoAutomaticoResponseDto>(json);
            }
            catch
            {
                return null;
            }

            if (response == null || response.Content == null)
                return null;

            bool liberado = !response.Content.Erro;

            string nomeCliente = ObterNomeCliente(response.Content);

            return new AcessoAutomatico
            {
                CodigoCliente = response.Content.CodigoCliente,
                NomeCliente = nomeCliente,
                Liberado = liberado,
                Mensagem = response.Content.Mensagem,
                Motivo = response.Content.MotivoErro,
                Servico = response.Content.Servico,
                DataHora = DateTime.Now
            };
        }

        private string ObterNomeCliente(AcessoAutomaticoContentDto content)
        {
            if (content.CodigoCliente.HasValue && content.CodigoCliente.Value > 0)
            {
                string nomeBanco = _clienteLocalService.BuscarNomeCliente(content.CodigoCliente.Value);

                if (!string.IsNullOrWhiteSpace(nomeBanco))
                    return nomeBanco;

                return "Cliente " + content.CodigoCliente.Value;
            }

            return ExtrairNomeDaMensagem(content.Mensagem);
        }

        private string ExtrairNomeDaMensagem(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return "Cliente não identificado";

            int posicaoPonto = mensagem.IndexOf(".");

            if (posicaoPonto > 0)
                return mensagem.Substring(0, posicaoPonto).Trim();

            return mensagem.Trim();
        }
    }
}