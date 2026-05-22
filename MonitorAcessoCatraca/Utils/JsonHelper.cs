using Newtonsoft.Json.Linq;

namespace MonitorAcessoCatraca.Utils
{
    public static class JsonHelper
    {
        public static int LerInt(JToken item, string campo)
        {
            try
            {
                JToken token = item[campo];

                if (token == null || token.Type == JTokenType.Null)
                    return 0;

                int valor;

                if (int.TryParse(token.ToString(), out valor))
                    return valor;

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static int? LerIntNullable(JToken item, string campo)
        {
            try
            {
                JToken token = item[campo];

                if (token == null || token.Type == JTokenType.Null)
                    return null;

                int valor;

                if (int.TryParse(token.ToString(), out valor))
                    return valor;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool LerBool(JToken item, string campo)
        {
            try
            {
                JToken token = item[campo];

                if (token == null || token.Type == JTokenType.Null)
                    return false;

                bool valor;

                if (bool.TryParse(token.ToString(), out valor))
                    return valor;

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string LerString(JToken item, string campo)
        {
            try
            {
                JToken token = item[campo];

                if (token == null || token.Type == JTokenType.Null)
                    return "";

                return token.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}