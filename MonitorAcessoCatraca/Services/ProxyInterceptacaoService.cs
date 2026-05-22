using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace MonitorAcessoCatraca.Services
{
    public class ProxyInterceptacaoService
    {
        private ProxyServer proxyServer;
        private ExplicitProxyEndPoint explicitEndPoint;
        private readonly AcessoAutomaticoParserService parserService;

        private bool iniciado = false;

        public event Action<AcessoAutomatico> AcessoCapturado;

        public ProxyInterceptacaoService(AcessoAutomaticoParserService parserService)
        {
            this.parserService = parserService;
        }

        public void Iniciar()
        {
            if (iniciado)
                return;

            proxyServer = new ProxyServer();

            proxyServer.CertificateManager.EnsureRootCertificate();
            proxyServer.CertificateManager.TrustRootCertificate(true);

            proxyServer.BeforeResponse += OnBeforeResponse;

            explicitEndPoint = new ExplicitProxyEndPoint(
                IPAddress.Loopback,
                AppConfig.PortaProxy,
                true
            );

            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

            proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

            iniciado = true;
        }

        public void Parar()
        {
            if (!iniciado)
                return;

            try
            {
                proxyServer.BeforeResponse -= OnBeforeResponse;

                proxyServer.DisableAllSystemProxies();
                proxyServer.Stop();
                proxyServer.Dispose();
            }
            catch
            {
            }
            finally
            {
                iniciado = false;
            }
        }

        private async Task OnBeforeResponse(object sender, SessionEventArgs e)
        {
            try
            {
                if (e == null || e.HttpClient == null || e.HttpClient.Request == null)
                    return;

                Uri uri = e.HttpClient.Request.RequestUri;

                if (uri == null)
                    return;

                bool hostCorreto = uri.Host.Equals(AppConfig.HostAcesso, StringComparison.OrdinalIgnoreCase);
                bool endpointCorreto = uri.AbsolutePath.Equals(AppConfig.EndpointAcessoAutomatico, StringComparison.OrdinalIgnoreCase);

                if (!hostCorreto || !endpointCorreto)
                    return;

                string json = await e.GetResponseBodyAsString();

                if (string.IsNullOrWhiteSpace(json))
                    return;

                AcessoAutomatico acesso = parserService.ConverterJsonParaAcesso(json);

                if (acesso == null)
                    return;

                AcessoCapturado?.Invoke(acesso);
            }
            catch
            {
                // Não trava o proxy por erro de leitura ou parse.
            }
        }
    }
}