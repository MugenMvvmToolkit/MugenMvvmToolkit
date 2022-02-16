using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class BindingMugenExtensions
    {
        private static readonly Type[] ConvertibleTypeArgs = new Type[1];
        private static readonly Dictionary<KeyValuePair<Type, Type>, bool> ConvertibleCache = new(23, InternalEqualityComparer.TypeType);
        private static readonly Dictionary<Type, object?> DefaultValueCache = new(23, InternalEqualityComparer.Type);
#if NET461
        private static readonly Dictionary<Type, bool> IsByRefLikeCache = new(23, InternalEqualityComparer.Type);
#endif
        public static Expression GenerateExpression(this Expression left, Expression right, Func<Expression, Expression, Expression> getExpr)
        {
            if (left.Type != right.Type && left.Type.IsValueType == right.Type.IsValueType)
            {
                if (right.Type.GetTypeConvertPriority() > left.Type.GetTypeConvertPriority())
                    return TryGenerate(left, right, right.Type, left.Type, getExpr);
            }

            return TryGenerate(left, right, left.Type, right.Type, getExpr);
        }

        public static void Convert(ref Expression left, ref Expression right, bool exactly)
        {
            if (left.Type == right.Type)
                return;
            if (left.Type.IsValueType != right.Type.IsValueType)
                return;
            if (left.Type.IsCompatibleWith(right.Type))
                left = left.ConvertIfNeed(right.Type, exactly);
            else if (right.Type.IsCompatibleWith(left.Type))
                right = right.ConvertIfNeed(left.Type, exactly);
        }

        public static bool IsParamArray(this IParameterInfo parameter)
        {
            Should.NotBeNull(parameter, nameof(parameter));
            return parameter.ParameterType.IsArray && parameter.IsDefined(typeof(ParamArrayAttribute));
        }

        public static ItemOrArray<Type> TryInferGenericParameters<TParameter, TArg>(ItemOrIReadOnlyList<Type> genericArguments, ItemOrIReadOnlyList<TParameter> parameters,
            Func<TParameter, Type> getParameterType, TArg args, Func<TArg, int, Type?> getArgumentType, int argsLength, out bool hasUnresolved)
        {
            Should.NotBeNull(getParameterType, nameof(getParameterType));
            Should.NotBeNull(getArgumentType, nameof(getArgumentType));
            hasUnresolved = false;
            var count = parameters.Count > argsLength ? argsLength : parameters.Count;
            var inferredTypes = ItemOrArray.Get<Type>(genericArguments.Count);
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

                inferredTypes.SetAt(i, inferred);
            }

            for (var i = 0; i < genericArguments.Count; i++)
            {
                var inferredType = inferredTypes[i];
                var arg = genericArguments[i];
                if (inferredType == arg)
                    continue;
                if (!IsCompatible(inferredType, arg.GenericParameterAttributes))
                    return default;
                var constraints = arg.GetGenericParameterConstraints();
                for (var j = 0; j < constraints.Length; j++)
                {
                    if (!constraints[j].IsAssignableFrom(inferredType))
                        return default;
                }
            }

            return inferredTypes;
        }

        public static bool IsCompatibleWith(this Type source, Type target) => IsCompatibleWith(source, target, out _);

        public static bool IsCompatibleWith(this Type source, Type target, out bool boxRequired)
        {
            boxRequired = false;
            if (source == target)
                return true;
            if (!target.IsValueType)
            {
                boxRequired = source.IsValueType;
                return target.IsAssignableFrom(source) || IsConvertible(target, source);
            }

            var st = GetNonNullableType(source);
            var tt = GetNonNullableType(target);
            if (st == tt)
                return true;
            if (st != source && tt == st)
                return IsConvertible(tt, st);

            var sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            var tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
            if (tc == TypeCode.Object && !tt.IsEnum)
                return IsConvertible(tt, st);

            switch (sc)
            {
                case TypeCode.SByte:
                    switch (tc)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Byte:
                    switch (tc)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Char:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int16:
                    switch (tc)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt16:
                case TypeCode.Char:
                    switch (tc)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Char:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int32:
                    switch (tc)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt32:
                    switch (tc)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int64:
                    switch (tc)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt64:
                    switch (tc)
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Single:
                    switch (tc)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }

                    break;
            }

            return false;
        }

        public static bool IsNullableType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static Type GetNonNullableType(this Type type) => IsNullableType(type) ? type.GetGenericArguments()[0] : type;

        public static EnumFlags<MemberFlags> GetAccessModifiers(this MemberInfo member) =>
            member switch
            {
                FieldInfo f => f.GetAccessModifiers(),
                PropertyInfo p => p.GetAccessModifiers(),
                EventInfo e => e.GetAccessModifiers(),
                MethodBase m => m.GetAccessModifiers(),
                _ => default
            };

        public static EnumFlags<MemberFlags> GetAccessModifiers(this FieldInfo? fieldInfo)
        {
            if (fieldInfo == null)
                return MemberFlags.Instance;
            if (fieldInfo.IsStatic)
                return (fieldInfo.IsPublic ? MemberFlags.StaticPublic : MemberFlags.StaticNonPublic) | fieldInfo.GetObservableFlags();
            return (fieldInfo.IsPublic ? MemberFlags.InstancePublic : MemberFlags.InstanceNonPublic) | fieldInfo.GetObservableFlags();
        }

        public static EnumFlags<MemberFlags> GetAccessModifiers(this EventInfo? eventInfo)
        {
            if (eventInfo == null)
                return MemberFlags.Instance;
            return (eventInfo.GetAddMethod(true) ?? eventInfo.GetRemoveMethod(true)).GetAccessModifiers();
        }

        public static EnumFlags<MemberFlags> GetAccessModifiers(this PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return MemberFlags.Instance;
            return GetAccessModifiers((MemberInfo) (propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true))!) | propertyInfo.GetObservableFlags();
        }

        public static EnumFlags<MemberFlags> GetAccessModifiers(this MethodBase? method)
        {
            ParameterInfo[]? parameters = null;
            return method.GetAccessModifiers(false, ref parameters);
        }

        public static EnumFlags<MemberFlags> GetAccessModifiers(this MethodBase? method, bool checkExtension,
            [NotNullIfNotNull("extensionParameters")]
            ref ParameterInfo[]? extensionParameters)
        {
            if (method == null)
                return MemberFlags.Instance;
            if (!method.IsStatic)
                return (method.IsPublic ? MemberFlags.InstancePublic : MemberFlags.InstanceNonPublic) | method.GetObservableFlags();

            if (checkExtension && method.IsDefined(typeof(ExtensionAttribute), false))
            {
                extensionParameters ??= method.GetParameters();
                if (extensionParameters.Length != 0)
                    return (method.IsPublic ? MemberFlags.Extension | MemberFlags.Public : MemberFlags.Extension | MemberFlags.NonPublic) | method.GetObservableFlags();
            }

            return (method.IsPublic ? MemberFlags.StaticPublic : MemberFlags.StaticNonPublic) | method.GetObservableFlags();
        }

        internal static object? GetDefaultValue(this Type type)
        {
            if (typeof(bool) == type)
                return BoxingExtensions.FalseObject;
            if (!typeof(ValueType).IsAssignableFrom(type))
                return null;
            if (!DefaultValueCache.TryGetValue(type, out var value))
            {
                value = Activator.CreateInstance(type);
                DefaultValueCache[type] = value;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasFlagEx(this GenericParameterAttributes attributes, GenericParameterAttributes flag) => (attributes & flag) == flag;

        internal static bool IsAssignableFromGeneric(this Type type, Type sourceType)
        {
            if (type.IsGenericTypeDefinition && FindCommonType(type, sourceType) != null)
                return true;
            return type.IsAssignableFrom(sourceType);
        }

        internal static HashSet<Type> SelfAndBaseTypes(Type type, bool addClasses = true, bool addInterfaces = true, HashSet<Type>? types = null)
        {
            types ??= new HashSet<Type>(InternalEqualityComparer.Type);
            types.Add(type);
            if (addClasses)
                AddSelfAndBaseClasses(types, type);
            if (addInterfaces)
                AddInterfaces(types, type, true);
            return types;
        }

        internal static EnumFlags<MemberFlags> GetObservableFlags(this MemberInfo member)
        {
            if (member.DeclaringType != null && member.DeclaringType.IsDefined(typeof(NonObservableAttribute), false) || member.IsDefined(typeof(NonObservableAttribute), false))
                return MemberFlags.NonObservable;
            return default;
        }

#if NET461
        internal static bool IsByRefLike(this Type type)
        {
            if (!type.IsValueType)
                return false;
            lock (IsByRefLikeCache)
            {
                if (!IsByRefLikeCache.TryGetValue(type, out var v))
                {
                    foreach (var attribute in type.GetCustomAttributes())
                    {
                        if ("System.Runtime.CompilerServices.IsByRefLikeAttribute" == attribute.GetType().FullName)
                        {
                            v = true;
                            break;
                        }
                    }

                    IsByRefLikeCache[type] = v;
                }

                return v;
            }
        }
#endif

        private static Expression TryGenerate(this Expression left, Expression right, Type firstType, Type secondType, Func<Expression, Expression, Expression> getExpr) =>
            TryGenerate(left, right, firstType, getExpr, false) ?? TryGenerate(left, right, secondType, getExpr, true)!;

        private static Expression? TryGenerate(this Expression left, Expression right, Type typeToConvert, Func<Expression, Expression, Expression> getExpr, bool throwOnError)
        {
            try
            {
                if ((left.Type.IsNullableType() || right.Type.IsNullableType()) && !typeToConvert.IsNullableType())
                    typeToConvert = typeof(Nullable<>).MakeGenericType(typeToConvert);
                return getExpr(left.ConvertIfNeed(typeToConvert, true), right.ConvertIfNeed(typeToConvert, true));
            }
            catch
            {
                if (throwOnError)
                    throw;
                return null;
            }
        }

        private static bool IsConvertible(Type typeFrom, Type typeTo)
        {
            var key = new KeyValuePair<Type, Type>(typeFrom, typeTo);
            lock (ConvertibleCache)
            {
                if (!ConvertibleCache.TryGetValue(key, out var v))
                {
                    ConvertibleTypeArgs[0] = typeTo;
                    v = typeFrom.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, null, ConvertibleTypeArgs, null) != null ||
                        typeFrom.GetMethod("op_Explicit", BindingFlags.Static | BindingFlags.Public, null, ConvertibleTypeArgs, null) != null;
                    ConvertibleCache[key] = v;
                }

                return v;
            }
        }

        private static int GetTypeConvertPriority(this Type type)
        {
            if (type.IsValueType)
            {
                type = type.GetNonNullableType();
                var typeCode = Type.GetTypeCode(type);
                if (typeCode == TypeCode.Object)
                    return 100;
                return (int) typeCode;
            }

            return 0;
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
                inputType = FindCommonType(parameterType.GetGenericTypeDefinition(), inputType)!;
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

        private static void AddSelfAndBaseClasses(HashSet<Type> types, Type type)
        {
            while (type != null)
            {
                types.Add(type);
                type = type.BaseType!;
            }
        }

        private static void AddInterfaces(HashSet<Type> types, Type type, bool isFirstCall)
        {
            if (!isFirstCall && type.IsInterface && types.Contains(type))
                return;

            types.Add(type);
            foreach (var t in type.GetInterfaces())
                AddInterfaces(types, t, false);
        }
    }
}