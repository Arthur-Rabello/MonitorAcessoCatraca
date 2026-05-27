using System;
using System.Collections.Generic;
using System.IO;

namespace MonitorAcessoCatraca.Config
{
    public static class AppConfig
    {
        public const string PASTA_CONTROLE_ACESSO = @"C:\Program Files (x86)\Next Fit\Controle de acesso";
        public const string CAMINHO_BANCO = @"C:\Program Files (x86)\Next Fit\Controle de acesso\banco.db3";

        public const string NOME_PROCESSO_CONTROLE_ACESSO = "ControleAcesso";
        public const string NOME_EXECUTAVEL_CONTROLE_ACESSO = "ControleAcesso.exe";

        public const int PORTA_PROXY = 8877;

        private static readonly Dictionary<string, string> _env = CarregarEnv();

        public static string HostAcesso
        {
            get { return ObterObrigatorio("HOST_ACESSO"); }
        }

        public static string EndpointAcessoAutomatico
        {
            get { return ObterObrigatorio("ENDPOINT_ACESSO_AUTOMATICO"); }
        }

        private static string ObterObrigatorio(string chave)
        {
            if (_env.ContainsKey(chave) && !string.IsNullOrWhiteSpace(_env[chave]))
                return _env[chave];

            throw new Exception("Variável obrigatória não encontrada no arquivo .env: " + chave);
        }

        private static Dictionary<string, string> CarregarEnv()
        {
            var resultado = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string caminhoEnv = EncontrarArquivoEnv();

            if (string.IsNullOrWhiteSpace(caminhoEnv))
            {
                throw new Exception("Arquivo .env não encontrado. Crie o arquivo .env na pasta do projeto ou na pasta do executável.");
            }
            foreach (string linhaOriginal in File.ReadAllLines(caminhoEnv))
            {
                string linha = linhaOriginal.Trim();

                if (string.IsNullOrWhiteSpace(linha))
                {
                    continue;
                }
                if (linha.StartsWith("#"))
                {
                    continue;
                }
                int posicaoIgual = linha.IndexOf('=');

                if (posicaoIgual <= 0)
                {
                    continue;
                }
                string chave = linha.Substring(0, posicaoIgual).Trim();
                string valor = linha.Substring(posicaoIgual + 1).Trim().Trim('"');

                resultado[chave] = valor;
            }

            return resultado;
        }

        private static string EncontrarArquivoEnv()
        {
            string[] caminhosPossiveis =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
                Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.env"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.env"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\.env")
            };

            foreach (string caminho in caminhosPossiveis)
            {
                string caminhoCompleto = Path.GetFullPath(caminho);

                if (File.Exists(caminhoCompleto))
                {
                    return caminhoCompleto;
                }
            }

                return null;
        }
    }
}