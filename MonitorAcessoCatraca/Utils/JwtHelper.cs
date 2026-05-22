using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace MonitorAcessoCatraca.Utils
{
    public static class JwtHelper
    {
        public static string ExtrairCodigoUnidade(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                var partes = token.Split('.');

                if (partes.Length < 2)
                    return null;

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

                byte[] bytes = Convert.FromBase64String(payload);
                string json = Encoding.UTF8.GetString(bytes);

                JObject obj = JObject.Parse(json);

                string unidade = null;

                if (obj["codigoUnidadePreferencial"] != null)
                    unidade = obj["codigoUnidadePreferencial"].ToString();

                if (string.IsNullOrWhiteSpace(unidade) && obj["codigoTenant"] != null)
                    unidade = obj["codigoTenant"].ToString();

                return unidade;
            }
            catch
            {
                return null;
            }
        }
    }
}