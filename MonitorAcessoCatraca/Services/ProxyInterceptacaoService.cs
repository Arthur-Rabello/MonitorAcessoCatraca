using MonitorAcessoCatraca.Config;
using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace MonitorAcessoCatraca.Services
{
    public class ProxyInterceptacaoService
    {
        private ProxyServer proxyServer;
        private ExplicitProxyEndPoint explicitEndPoint;

        private bool iniciado = false;

        public event Action AcessoAutomaticoDetectado;

        public void Iniciar()
        {
            if (iniciado)
                return;

            proxyServer = new ProxyServer();

            string caminhoCertificado = ObterCaminhoCertificado();

            try
            {
                proxyServer.CertificateManager.PfxFilePath = caminhoCertificado;
            }
            catch
            {
                // Caso a versão do Titanium não tenha PfxFilePath.
            }

            proxyServer.CertificateManager.EnsureRootCertificate();

            try
            {
                X509Certificate2 certificado = proxyServer.CertificateManager.RootCertificate;

                if (!CertificadoEstaConfiavel(certificado))
                {
                    proxyServer.CertificateManager.TrustRootCertificate(false);
                }
            }
            catch
            {
            }

            proxyServer.BeforeResponse += OnBeforeResponse;

            explicitEndPoint = new ExplicitProxyEndPoint(
                IPAddress.Loopback,
                AppConfig.PORTA_PROXY,
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

        private Task OnBeforeResponse(object sender, SessionEventArgs e)
        {
            try
            {
                if (e == null || e.HttpClient == null || e.HttpClient.Request == null)
                    return Task.CompletedTask;

                Uri uri = e.HttpClient.Request.RequestUri;

                if (uri == null)
                    return Task.CompletedTask;

                bool hostCorreto = uri.Host.Equals(
                    AppConfig.HostAcesso,
                    StringComparison.OrdinalIgnoreCase
                );

                if (!hostCorreto)
                    return Task.CompletedTask;

                bool endpointCorreto = uri.AbsolutePath.IndexOf(
                    AppConfig.EndpointAcessoAutomatico,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;

                if (!endpointCorreto)
                    return Task.CompletedTask;

                AcessoAutomaticoDetectado?.Invoke();
            }
            catch
            {

            }

            return Task.CompletedTask;
        }
        private string ObterPastaCertificado()
        {
            string pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Next Fit",
                "MonitorAcessoCatraca",
                "Certificado"
            );

            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            return pasta;
        }

        private string ObterCaminhoCertificado()
        {
            return Path.Combine(ObterPastaCertificado(), "rootCert.pfx");
        }

        private bool CertificadoEstaConfiavel(X509Certificate2 certificado)
        {
            if (certificado == null)
                return false;

            try
            {
                using (X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);

                    foreach (X509Certificate2 cert in store.Certificates)
                    {
                        if (
                            cert.Thumbprint != null &&
                            certificado.Thumbprint != null &&
                            cert.Thumbprint.Equals(certificado.Thumbprint, StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }
}