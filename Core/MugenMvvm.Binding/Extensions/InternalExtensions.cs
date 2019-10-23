using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Enums;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Fields

        internal static readonly char[] CommaSeparator = { ',' };
        internal static readonly char[] DotSeparator = { '.' };

        #endregion

        #region Methods

        internal static string GetPath(this StringBuilder memberNameBuilder)
        {
            if (memberNameBuilder.Length != 0 && memberNameBuilder[0] == '.')
                memberNameBuilder.Remove(0, 1);
            return memberNameBuilder.ToString();
        }

        internal static bool HasFlagEx(this GenericParameterAttributes attributes, GenericParameterAttributes flag)
        {
            return (attributes & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNullOrUnsetValue(this object? value)
        {
            return value == null || ReferenceEquals(value, BindingMetadata.UnsetValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValueOrDoNothing(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.UnsetValue) || ReferenceEquals(value, BindingMetadata.DoNothing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValue(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.UnsetValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDoNothing(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.DoNothing);
        }

        internal static HashSet<Type> SelfAndBaseTypes(Type type)
        {
            var types = new HashSet<Type>(SelfAndBaseClasses(type));
            AddInterface(types, type, true);
            return types;
        }

        internal static string[]? GetIndexerValuesRaw(string path)
        {
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                path = path.Substring(4);
            if (!path.StartsWith("[", StringComparison.Ordinal) || !path.EndsWith("]", StringComparison.Ordinal))
                return null;

            return path
                .RemoveBounds()
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static object?[] GetIndexerValues(string path, ParameterInfo[]? parameters = null, Type? castType = null)
        {
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                path = path.Substring(4);
            if (!path.StartsWith("[", StringComparison.Ordinal))
                return Default.EmptyArray<object>();
            return GetIndexerValues(path
                .RemoveBounds()
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries), parameters, castType);
        }

        internal static TItem[] GetIndexerValues<TItem>(string[] args)
        {
            var result = new TItem[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = (TItem)(s == "null" ? null : MugenBindingService.GlobalValueConverter.Convert(s, typeof(TItem)))!;
            }

            return result;
        }

        internal static object?[] GetIndexerValues(string[] args, ParameterInfo[]? parameters = null, Type? castType = null)
        {
            if (parameters == null)
                Should.NotBeNull(castType, nameof(castType));
            else
                Should.NotBeNull(parameters, nameof(parameters));
            var result = new object?[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (parameters != null)
                    castType = parameters[i].ParameterType;
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = s == "null" ? null : MugenBindingService.GlobalValueConverter.Convert(s, castType ?? typeof(object));
            }

            return result;
        }

        internal static string RemoveBounds(this string st) //todo Span?
        {
            return st.Substring(1, st.Length - 2);
        }

        internal static MemberFlags GetAccessModifiers(this MethodBase? method)
        {
            if (method == null)
                return MemberFlags.Instance;
            if (method.IsStatic)
                return method.IsPublic ? MemberFlags.StaticPublic : MemberFlags.StaticNonPublic;
            return method.IsPublic ? MemberFlags.InstancePublic : MemberFlags.InstanceNonPublic;
        }

        private static void ToStringValue(this IExpressionNode expression, StringBuilder builder)
        {
            var constantExpressionNode = (IConstantExpressionNode)expression;
            var value = constantExpressionNode.Value;

            if (value == null)
            {
                builder.Insert(0, "null");
                return;
            }

            if (value is string st)
            {
                builder.Insert(0, '"');
                builder.Insert(0, st);
                builder.Insert(0, '"');
                return;
            }

            builder.Insert(0, value);
        }

        private static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.GetBaseTypeUnified();
            }
        }

        private static void AddInterface(HashSet<Type> types, Type type, bool isFirstCall)
        {
            if (!isFirstCall && type.IsInterfaceUnified() && types.Contains(type))
                return;
            types.Add(type);
            foreach (var t in type.GetInterfacesUnified())
                AddInterface(types, t, false);
        }

        #endregion
    }
}