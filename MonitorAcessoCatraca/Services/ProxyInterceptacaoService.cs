using MonitorAcessoCatraca.Config;
using MonitorAcessoCatraca.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Microsoft.Win32;
using System.IO;

namespace MonitorAcessoCatraca.Services
{
    public class ProxyInterceptacaoService
    {
        private ProxyServer proxyServer;
        private ExplicitProxyEndPoint explicitEndPoint;
        private readonly AcessoAutomaticoParserService parserService;
        private string pacPath;
        private object valorAntigoProxyEnable;
        private object valorAntigoProxyServer;
        private object valorAntigoAutoConfigUrl;

        private bool iniciado = false;

        public event Action<AcessoAutomatico> AcessoCapturado;

        private void ConfigurarPacSomenteNextFit()
        {
            pacPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "monitor_nextfit_proxy.pac"
            );

            string pacConteudo =
                @"function FindProxyForURL(url, host) {
                        if (dnsDomainIs(host, ""acesso.nextfit.com.br"")) {
                            return ""PROXY 127.0.0.1:" + AppConfig.PortaProxy + @""";
                        }

                        return ""DIRECT"";
                    }";

            File.WriteAllText(pacPath, pacConteudo);

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                true
            ))
            {
                valorAntigoProxyEnable = key.GetValue("ProxyEnable");
                valorAntigoProxyServer = key.GetValue("ProxyServer");
                valorAntigoAutoConfigUrl = key.GetValue("AutoConfigURL");

                key.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                key.DeleteValue("ProxyServer", false);
                key.SetValue("AutoConfigURL", "file:///" + pacPath.Replace("\\", "/"), RegistryValueKind.String);
            }

            AtualizarConfiguracaoInternet();
        }

        private void RestaurarConfiguracaoProxyWindows()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                    true
                ))
                {
                    if (valorAntigoProxyEnable != null)
                        key.SetValue("ProxyEnable", valorAntigoProxyEnable, RegistryValueKind.DWord);
                    else
                        key.DeleteValue("ProxyEnable", false);

                    if (valorAntigoProxyServer != null)
                        key.SetValue("ProxyServer", valorAntigoProxyServer, RegistryValueKind.String);
                    else
                        key.DeleteValue("ProxyServer", false);

                    if (valorAntigoAutoConfigUrl != null)
                        key.SetValue("AutoConfigURL", valorAntigoAutoConfigUrl, RegistryValueKind.String);
                    else
                        key.DeleteValue("AutoConfigURL", false);
                }

                AtualizarConfiguracaoInternet();
            }
            catch
            {
            }
        }

        [System.Runtime.InteropServices.DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;

        private void AtualizarConfiguracaoInternet()
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

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
            ConfigurarPacSomenteNextFit();

            iniciado = true;
        }

        public void Parar()
        {
            if (!iniciado)
                return;

            try
            {
                RestaurarConfiguracaoProxyWindows();

                if (proxyServer != null)
                {
                    proxyServer.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;
                    proxyServer.BeforeResponse -= OnBeforeResponse;

                    proxyServer.Stop();
                    proxyServer.Dispose();
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