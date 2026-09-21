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
                {
                    return "";
                }

                string[] partes = token.Split('.');

                if (partes.Length < 2)
                {
                    return "";
                }

                string payload = partes[1];

                payload = payload.Replace('-', '+').Replace('_', '/');

                int resto = payload.Length % 4;

                if (resto > 0)
                {
                    payload = payload.PadRight(payload.Length + (4 - resto), '=');
                }

                byte[] bytes = Convert.FromBase64String(payload);
                string json = Encoding.UTF8.GetString(bytes);

                JObject obj = JObject.Parse(json);

                string codigoUnidade =
                    obj["codigoTenant"] != null ? obj["codigoTenant"].ToString() :
                    obj["codigoUnidadePreferencial"] != null ? obj["codigoUnidadePreferencial"].ToString() :
                    obj["codigoUnidade"] != null ? obj["codigoUnidade"].ToString() :
                    obj["CodigoUnidade"] != null ? obj["CodigoUnidade"].ToString() :
                    "";

                return codigoUnidade;
            }
            catch
            {
                return "";
            }
        }
    }
}