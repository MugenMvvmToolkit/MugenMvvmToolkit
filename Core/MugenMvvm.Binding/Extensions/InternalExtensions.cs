using System;
using System.Collections.Generic;
using System.Reflection;

namespace MugenMvvm.Binding.Extensions
{
    public static partial class BindingMugenExtensions
    {
        internal static readonly string[] CommaSeparator = { "," };

        #region Methods

        internal static object[] GetIndexerValues(string path, IList<ParameterInfo> parameters = null, Type castType = null)//todo Span?
        {
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                path = path.Substring(4);
            if (!path.StartsWith("[", StringComparison.Ordinal))
                return Default.EmptyArray<object>();
            var args = path
                .RemoveBounds()
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
            var result = new object[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (parameters != null)
                    castType = parameters[i].ParameterType;
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = s == "null" ? null : BindingServiceProvider.ValueConverter(BindingMemberInfo.Empty, castType, s);
            }

            return result;
        }

        internal static string RemoveBounds(this string st) //todo Span?
        {
            return st.Substring(1, st.Length - 2);
        }

        #endregion
    }
}