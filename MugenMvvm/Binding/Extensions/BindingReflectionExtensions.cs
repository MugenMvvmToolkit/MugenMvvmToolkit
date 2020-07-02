using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Extensions;

namespace MugenMvvm.Binding.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        private static readonly TypeLightDictionary<object?> DefaultValueCache = new TypeLightDictionary<object?>(23);

        #endregion

        #region Methods

        public static Expression GenerateExpression(this Expression left, Expression right, Func<Expression, Expression, Expression> getExpr)
        {
            Convert(ref left, ref right, true);
            return getExpr(left, right);
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
            return parameter.IsDefined(typeof(ParamArrayAttribute));
        }

        public static Type[]? TryInferGenericParameters<TParameter, TArg>(IReadOnlyList<Type> genericArguments, IReadOnlyList<TParameter> parameters,
            Func<TParameter, Type> getParameterType, TArg args, Func<TArg, int, Type?> getArgumentType, int argsLength, out bool hasUnresolved)
        {
            Should.NotBeNull(genericArguments, nameof(genericArguments));
            Should.NotBeNull(parameters, nameof(parameters));
            Should.NotBeNull(getParameterType, nameof(getParameterType));
            Should.NotBeNull(getArgumentType, nameof(getArgumentType));
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

        public static bool IsCompatibleWith(this Type source, Type target)
        {
            return IsCompatibleWith(source, target, out _);
        }

        public static bool IsCompatibleWith(this Type source, Type target, out bool boxRequired)
        {
            boxRequired = false;
            if (source == target)
                return true;
            if (!target.IsValueType)
            {
                boxRequired = source.IsValueType;
                return target.IsAssignableFrom(source);
            }

            var st = GetNonNullableType(source);
            var tt = GetNonNullableType(target);
            if (st != source && tt == st)
                return false;
            var sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            var tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
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

            return st == tt;
        }

        public static Type GetTargetType([MaybeNull] ref Expression target)
        {
            var type = target.Type;
            if (target is ConstantExpression constant && constant.Value is Type value)
            {
                type = value;
                target = null!;
            }

            return type;
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetNonNullableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        public static MemberFlags GetAccessModifiers(this MemberInfo member)
        {
            return member switch
            {
                FieldInfo f => f.GetAccessModifiers(),
                PropertyInfo p => p.GetAccessModifiers(),
                EventInfo e => e.GetAccessModifiers(),
                MethodBase m => m.GetAccessModifiers(),
                _ => 0
            };
        }

        public static MemberFlags GetAccessModifiers(this FieldInfo? fieldInfo)
        {
            if (fieldInfo == null)
                return MemberFlags.Instance;
            if (fieldInfo.IsStatic)
                return fieldInfo.IsPublic ? MemberFlags.StaticPublic : MemberFlags.StaticNonPublic;
            return fieldInfo.IsPublic ? MemberFlags.InstancePublic : MemberFlags.InstanceNonPublic;
        }

        public static MemberFlags GetAccessModifiers(this EventInfo? eventInfo)
        {
            if (eventInfo == null)
                return MemberFlags.Instance;
            return (eventInfo.GetAddMethod(true) ?? eventInfo.GetRemoveMethod(true)).GetAccessModifiers();
        }

        public static MemberFlags GetAccessModifiers(this PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return MemberFlags.Instance;
            return (propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true)).GetAccessModifiers();
        }

        public static MemberFlags GetAccessModifiers(this MethodBase? method)
        {
            ParameterInfo[]? parameters = null;
            return method.GetAccessModifiers(false, ref parameters);
        }

        public static MemberFlags GetAccessModifiers(this MethodBase? method, bool checkExtension, [NotNullIfNotNull("extensionParameters")]
            ref ParameterInfo[]? extensionParameters)
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
        internal static bool HasFlagEx(this GenericParameterAttributes attributes, GenericParameterAttributes flag)
        {
            return (attributes & flag) == flag;
        }

        internal static bool IsAssignableFromGeneric(this Type type, Type sourceType)
        {
            if (type.IsGenericTypeDefinition && FindCommonType(type, sourceType) != null)
                return true;
            return type.IsAssignableFrom(sourceType);
        }

        internal static HashSet<Type> SelfAndBaseTypes(Type type, bool addClasses = true, bool addInterfaces = true, HashSet<Type>? types = null)
        {
            if (types == null)
                types = new HashSet<Type>();
            types.Add(type);
            if (addClasses)
                AddSelfAndBaseClasses(types, type);
            if (addInterfaces)
                AddInterface(types, type, true);
            return types;
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
                type = type.BaseType;
            }
        }

        private static void AddInterface(HashSet<Type> types, Type type, bool isFirstCall)
        {
            if (!isFirstCall && type.IsInterface && types.Contains(type))
                return;
            types.Add(type);
            var interfaces = type.GetInterfaces();
            for (var index = 0; index < interfaces.Length; index++)
                AddInterface(types, interfaces[index], false);
        }

        #endregion
    }
}