using System;
using System.ComponentModel;
using System.Reflection;

namespace MonitorAcessoCatraca.Utils
{
    public static class EnumHelper
    {
        public static string ObterDescricao(Enum valor)
        {
            if (valor == null)
                return "";

            FieldInfo campo = valor.GetType().GetField(valor.ToString());

            if (campo == null)
                return valor.ToString();

            DescriptionAttribute atributo = campo.GetCustomAttribute<DescriptionAttribute>();

            if (atributo == null)
                return valor.ToString();

            return atributo.Description;
        }
    }
}