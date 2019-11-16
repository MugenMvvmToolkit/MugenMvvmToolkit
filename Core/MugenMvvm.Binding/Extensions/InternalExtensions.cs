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

        internal static bool EqualsEx(this IMemberInfo x, IMemberInfo y)
        {
            if (x.MemberType != y.MemberType || x.Name != y.Name || x.DeclaringType != y.DeclaringType)
                return false;

            if (x.MemberType != MemberType.Method)
                return true;

            var xM = ((IMethodInfo)x).GetParameters();
            var yM = ((IMethodInfo)y).GetParameters();
            if (xM.Count != yM.Count)
                return false;

            for (var i = 0; i < xM.Count; i++)
            {
                if (xM[i].ParameterType != yM[i].ParameterType)
                    return false;
            }

            return true;
        }

        internal static int GetHashCodeEx(this IMemberInfo memberInfo)
        {
            unchecked
            {
                return memberInfo.DeclaringType.GetHashCode() * 397 ^ (int)memberInfo.MemberType * 397 ^ memberInfo.Name.GetHashCode();
            }
        }

        internal static T[] InsertFirstArg<T>(this T[] args, T firstArg)
        {
            if (args == null || args.Length == 0)
                return new[] { firstArg };
            var objects = new T[args.Length + 1];
            objects[0] = firstArg;
            Array.Copy(args, 0, objects, 1, args.Length);
            return objects;
        }

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
            var types = new HashSet<Type>();
            AddSelfAndBaseClasses(types, type);
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
            converter = converter.DefaultIfNull();
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
            Type? castType, IReadOnlyMetadataContext? metadata, int parametersStartIndex = 0)
        {
            if (parameters == null)
                Should.NotBeNull(castType, nameof(castType));
            else
                Should.NotBeNull(parameters, nameof(parameters));
            if (args.Length == 0)
                return Default.EmptyArray<object?>();

            converter = converter.DefaultIfNull();
            var result = new object?[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (parameters != null)
                    castType = parameters[i + parametersStartIndex].ParameterType;
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = s == "null" ? null : converter.Convert(s, castType!, parameters?[i + parametersStartIndex], metadata);
            }

            return result;
        }

        internal static MemberFlags GetAccessModifiers(this EventInfo? eventInfo)
        {
            if (eventInfo == null)
                return MemberFlags.Instance;
            return (eventInfo.GetAddMethod(true) ?? eventInfo.GetRemoveMethod(true)).GetAccessModifiers();
        }

        internal static MemberFlags GetAccessModifiers(this PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return MemberFlags.Instance;
            return (propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true)).GetAccessModifiers();
        }

        internal static MemberFlags GetAccessModifiers(this MethodBase? method)
        {
            ParameterInfo[]? parameters = null;
            return method.GetAccessModifiers(false, ref parameters);
        }

        internal static MemberFlags GetAccessModifiers(this MethodBase? method, bool checkExtension, ref ParameterInfo[]? extensionParameters)
        {
            if (method == null)
                return MemberFlags.Instance;
            if (!method.IsStatic)
                return method.IsPublic ? MemberFlags.InstancePublic : MemberFlags.InstanceNonPublic;

            if (checkExtension && method.IsDefined(typeof(ExtensionAttribute), false))
            {
                if (extensionParameters == null)
                    extensionParameters = method.GetParameters();
                if (extensionParameters.Length != 0)
                    return method.IsPublic ? MemberFlags.Extension | MemberFlags.Public : MemberFlags.Extension | MemberFlags.NonPublic;
            }
            return method.IsPublic ? MemberFlags.StaticPublic : MemberFlags.StaticNonPublic;
        }

        internal static Type[]? TryInferGenericParameters<TParameter, TArg>(IReadOnlyList<Type> genericArguments, IReadOnlyList<TParameter> parameters, Func<TParameter, Type> getParameterType,
            TArg args, Func<TArg, int, Type?> getArgumentType, int argsLength, out bool hasUnresolved)
        {
            hasUnresolved = false;
            var count = parameters.Count > argsLength ? argsLength : parameters.Count;
            var inferredTypes = new Type[genericArguments.Count];
            for (var i = 0; i < genericArguments.Count; i++)
            {
                var argument = genericArguments[i];
                Type? inferred = null;
                if (argument.IsGenericParameter)
                {
                    for (var index = 0; index < count; index++)
                    {
                        var argType = getArgumentType(args, index);
                        if (argType != null)
                        {
                            inferred = argument.TryInferGenericParameter(getParameterType(parameters[index]), argType);
                            if (inferred != null)
                                break;
                        }
                    }
                }
                else
                    inferred = argument;

                if (inferred == null)
                {
                    inferred = argument;
                    hasUnresolved = true;
                }

                inferredTypes[i] = inferred;
            }

            for (var i = 0; i < genericArguments.Count; i++)
            {
                var inferredType = inferredTypes[i];
                var arg = genericArguments[i];
                if (ReferenceEquals(inferredType, arg))
                    continue;
                if (!IsCompatible(inferredType, arg.GenericParameterAttributes))
                    return null;
                var constraints = arg.GetGenericParameterConstraints();
                for (var j = 0; j < constraints.Length; j++)
                {
                    if (!constraints[j].IsAssignableFrom(inferredType))
                        return null;
                }
            }

            return inferredTypes;
        }

        internal static bool IsAssignableFromGeneric(this Type type, Type sourceType)
        {
            if (type.IsGenericTypeDefinition && FindCommonType(type, sourceType) != null)
                return true;
            return type.IsAssignableFrom(sourceType);
        }

        private static Type? FindCommonType(Type genericDefinition, Type type)
        {
            foreach (var baseType in SelfAndBaseTypes(type))
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericDefinition)
                    return baseType;
            }

            return null;
        }

        private static Type? TryInferGenericParameter(this Type genericArgument, Type parameterType, Type inputType)
        {
            if (parameterType == genericArgument)
                return inputType;
            if (parameterType.IsArray)
                return inputType.IsArray ? inputType.GetElementType() : null;

            if (parameterType.IsGenericType)
            {
                inputType = MugenBindingExtensions.FindCommonType(parameterType.GetGenericTypeDefinition(), inputType)!;
                if (inputType == null)
                    return null;

                var srcArgs = parameterType.GetGenericArguments();
                var inputArgs = inputType.GetGenericArguments();
                for (var index = 0; index < srcArgs.Length; index++)
                {
                    var parameter = genericArgument.TryInferGenericParameter(srcArgs[index], inputArgs[index]);
                    if (parameter != null)
                        return parameter;
                }
            }

            return null;
        }

        private static bool IsCompatible(Type type, GenericParameterAttributes attributes)
        {
            if (attributes.HasFlagEx(GenericParameterAttributes.ReferenceTypeConstraint) && type.IsValueType)
                return false;
            if (attributes.HasFlagEx(GenericParameterAttributes.NotNullableValueTypeConstraint) && !type.IsValueType)
                return false;
            return true;
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

        private static void AddSelfAndBaseClasses(HashSet<Type> types, Type type)
        {
            while (type != null)
            {
                types.Add(type);
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