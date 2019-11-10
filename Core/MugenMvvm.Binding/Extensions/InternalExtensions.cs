using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        internal static readonly char[] CommaSeparator = { ',' };
        internal static readonly char[] DotSeparator = { '.' };

        #endregion

        #region Methods

        internal static void AddMethodObserver(this ObserverBase.IMethodPathObserver observer, object? target, IMemberInfo? lastMember, ref ActionToken unsubscriber, ref IWeakReference? lastValueRef)
        {
            unsubscriber.Dispose();
            if (target == null || !(lastMember is IMemberAccessorInfo propertyInfo))
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            var value = propertyInfo.GetValue(target);
            if (ReferenceEquals(value, lastValueRef?.Target))
                return;

            var type = value?.GetType()!;
            if (value.IsNullOrUnsetValue() || type.IsValueType)
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            lastValueRef = value.ToWeakReference();
            var memberFlags = observer.MemberFlags & ~MemberFlags.Static;
            var member = MugenBindingService.MemberProvider.GetMember(type!, observer.Method, MemberType.Method, memberFlags);
            if (member is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(target, observer.GetMethodListener());
            if (unsubscriber.IsEmpty)
                unsubscriber = ActionToken.NoDoToken;
        }

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

        internal static string[]? GetIndexerArgsRaw(string path)
        {
            int start = 1;
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                start = 5;
            else if (!path.StartsWith("[", StringComparison.Ordinal) || !path.EndsWith("]", StringComparison.Ordinal))
                return null;

            return path
                .RemoveBounds(start)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static string[]? GetMethodArgsRaw(string path, out string? methodName)
        {
            var startIndex = path.IndexOf('(');
            if (startIndex < 0 || !path.EndsWith(")", StringComparison.Ordinal))
            {
                methodName = null;
                return null;
            }

            methodName = path.Substring(0, startIndex);
            return path
                .RemoveBounds(startIndex + 1)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static TItem[] ConvertValues<TItem>(this IGlobalValueConverter? converter, string[] args, IReadOnlyMetadataContext? metadata)
        {
            converter = converter.ServiceIfNull();
            var result = new TItem[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = (TItem)(s == "null" ? null : converter.Convert(s, typeof(TItem), metadata: metadata))!;
            }

            return result;
        }

        internal static object?[] ConvertValues(this IGlobalValueConverter? converter, string[] args, ParameterInfo[]? parameters,
            Type? castType, IReadOnlyMetadataContext? metadata)
        {
            if (parameters == null)
                Should.NotBeNull(castType, nameof(castType));
            else
                Should.NotBeNull(parameters, nameof(parameters));
            if (args.Length == 0)
                return Default.EmptyArray<object?>();

            converter = converter.ServiceIfNull();
            var result = new object?[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (parameters != null)
                    castType = parameters[i].ParameterType;
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = s == "null" ? null : converter.Convert(s, castType!, parameters?[i], metadata);
            }

            return result;
        }

        internal static MemberFlags GetAccessModifiers(this MethodBase? method)
        {
            if (method == null)
                return MemberFlags.Instance;
            if (method.IsStatic)
                return method.IsPublic ? MemberFlags.StaticPublic : MemberFlags.StaticNonPublic;
            return method.IsPublic ? MemberFlags.InstancePublic : MemberFlags.InstanceNonPublic;
        }

        internal static IMemberInfo? FindBestMember(List<KeyValuePair<Type, IMemberInfo>> members)
        {
            if (members.Count == 0)
                return null;
            if (members.Count == 1)
                return members[0].Value;

            for (int i = 0; i < members.Count; i++)
            {
                KeyValuePair<Type, IMemberInfo> currentMemberPair = members[i];
                bool isInterface = currentMemberPair.Key.IsInterface;
                for (int j = 0; j < members.Count; j++)
                {
                    if (i == j)
                        continue;
                    var nextMemberPair = members[j];
                    if (isInterface && currentMemberPair.Key.IsAssignableFrom(nextMemberPair.Key))
                    {
                        members.RemoveAt(i);
                        i--;
                        break;
                    }

                    if (nextMemberPair.Key.IsSubclassOf(currentMemberPair.Key))
                    {
                        members.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
            return members[0].Value;
        }


        internal static Type? FindCommonType(Type genericDefinition, Type type)
        {
            foreach (var baseType in SelfAndBaseTypes(type))
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericDefinition)
                    return baseType;
            }

            return null;
        }

        private static string RemoveBounds(this string st, int start = 1) //todo Span?
        {
            return st.Substring(start, st.Length - start - 1);
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
                type = type.BaseType;
            }
        }

        private static void AddInterface(HashSet<Type> types, Type type, bool isFirstCall)
        {
            if (!isFirstCall && type.IsInterface && types.Contains(type))
                return;
            types.Add(type);
            foreach (var t in type.GetInterfaces())
                AddInterface(types, t, false);
        }

        #endregion
    }
}