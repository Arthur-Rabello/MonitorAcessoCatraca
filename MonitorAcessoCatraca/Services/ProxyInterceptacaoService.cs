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

            try
            {
                proxyServer.CertificateManager.TrustRootCertificate(true);
            }
            catch
            {
                
            }

            proxyServer.BeforeResponse += OnBeforeResponse;

            explicitEndPoint = new ExplicitProxyEndPoint(
                IPAddress.Loopback,
                AppConfig.PortaProxy,
                true
            );

            explicitEndPoint.BeforeTunnelConnectRequest += FiltrarTunnelConnectRequest;

            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

            proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

            iniciado = true;
        }

        public void Parar()
        {
            try
            {
                if (explicitEndPoint != null)
                {
                    try
                    {
                        explicitEndPoint.BeforeTunnelConnectRequest -= FiltrarTunnelConnectRequest;
                    }
                    catch
                    {
                    }
                }

                if (proxyServer != null)
                {
                    try
                    {
                        proxyServer.BeforeResponse -= OnBeforeResponse;
                    }
                    catch
                    {
                    }

                    try
                    {
                        proxyServer.DisableAllSystemProxies();
                    }
                    catch
                    {
                    }

                    try
                    {
                        proxyServer.Stop();
                    }
                    catch
                    {
                    }

                    try
                    {
                        proxyServer.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            finally
            {
                proxyServer = null;
                explicitEndPoint = null;
                iniciado = false;

                ProxyWindowsService.DesativarProxyWindows();
            }
        }

        private Task FiltrarTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            bool deveDescriptografar = false;

            try
            {
                if (e == null || e.HttpClient == null || e.HttpClient.Request == null)
                {
                    e.DecryptSsl = false;
                    return Task.CompletedTask;
                }

                Uri uri = e.HttpClient.Request.RequestUri;

                if (uri == null)
                {
                    e.DecryptSsl = false;
                    return Task.CompletedTask;
                }

                string host = uri.Host;

                if (host.Equals(AppConfig.HostAcesso, StringComparison.OrdinalIgnoreCase))
                    deveDescriptografar = true;

                e.DecryptSsl = deveDescriptografar;
            }
            catch
            {
                e.DecryptSsl = false;
            }

            return Task.CompletedTask;
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

                bool hostCorreto = uri.Host.Equals(
                    AppConfig.HostAcesso,
                    StringComparison.OrdinalIgnoreCase
                );

                if (!hostCorreto)
                    return;

                bool endpointCorreto = uri.AbsolutePath.IndexOf(
                    AppConfig.EndpointAcessoAutomatico,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;

                if (!endpointCorreto)
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
                
            }
        }
    }
}