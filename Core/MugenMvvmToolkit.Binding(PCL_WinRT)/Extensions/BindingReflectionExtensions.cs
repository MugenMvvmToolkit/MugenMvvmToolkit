#region Copyright
// ****************************************************************************
// <copyright file="BindingReflectionExtensions.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding
{
    internal static class BindingReflectionExtensions
    {
        #region Nested types

#if PCL_WINRT
        private enum TypeCode
        {
            Byte,
            Int16,
            Int32,
            Int64,
            SByte,
            UInt16,
            UInt32,
            UInt64,
            Single,
            Double,
            Char,
            Boolean,
            String,
            DateTime,
            Decimal,
            Empty,
            Object
        }
#endif

        #endregion

        #region Fields

#if PCL_WINRT
        private static readonly Dictionary<Type, TypeCode> TypeCodeTable;
#endif
        private static readonly Dictionary<Type, List<MethodInfo>> TypeToExtensionMethods;
        private static readonly Func<MethodInfo, bool> IsExtensionMethodDelegate;

        #endregion

        #region Constructors

        static BindingReflectionExtensions()
        {
#if PCL_WINRT
            TypeCodeTable = new Dictionary<Type, TypeCode>
            {
                {typeof (Boolean), TypeCode.Boolean},
                {typeof (Char), TypeCode.Char},
                {typeof (Byte), TypeCode.Byte},
                {typeof (Int16), TypeCode.Int16},
                {typeof (Int32), TypeCode.Int32},
                {typeof (Int64), TypeCode.Int64},
                {typeof (SByte), TypeCode.SByte},
                {typeof (UInt16), TypeCode.UInt16},
                {typeof (UInt32), TypeCode.UInt32},
                {typeof (UInt64), TypeCode.UInt64},
                {typeof (Single), TypeCode.Single},
                {typeof (Double), TypeCode.Double},
                {typeof (DateTime), TypeCode.DateTime},
                {typeof (Decimal), TypeCode.Decimal},
                {typeof (String), TypeCode.String},
            };
#endif
            TypeToExtensionMethods = new Dictionary<Type, List<MethodInfo>>();
            IsExtensionMethodDelegate = IsExtensionMethod;
        }

        #endregion

        #region Methods

        public static bool IsOverride(this MethodInfo method, Type baseType)
        {
            return method != null && method.DeclaringType != baseType;
        }

        internal static object GetDefaultValue(this Type type)
        {
            return type.IsValueType() ? Activator.CreateInstance(type) : null;
        }

        internal static object Convert(Type type, object value)
        {
            if (type.IsInstanceOfType(value) || value == null)
                return value;
#if PCL_WINRT
            if (TypeCodeTable.ContainsKey(value.GetType()))
#else
            if (value is IConvertible)
#endif
                return System.Convert.ChangeType(value, type.GetNonNullableType(), CultureInfo.CurrentCulture);
            if (type == typeof(string))
                return value.ToString();
            return value;
        }

        internal static Func<object, object> GetGetPropertyAccessor(this PropertyInfo propertyInfo, MethodInfo getMethod, string path)
        {
            Should.NotBeNull(propertyInfo, "propertyInfo");
            Should.NotBeNull(getMethod, "getMethod");
            ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
            if (indexParameters.Length == 0)
                return ServiceProvider.ReflectionManager.GetMemberGetter<object>(propertyInfo);
            Func<object, object[], object> @delegate = ServiceProvider
                .ReflectionManager
                .GetMethodDelegate(getMethod);
            object[] indexerValues = GetIndexerValues(path, indexParameters);
            return o => @delegate(o, indexerValues);
        }

        internal static Action<object, object> GetSetPropertyAccessor(this PropertyInfo propertyInfo, MethodInfo setMethod, string path)
        {
            Should.NotBeNull(propertyInfo, "propertyInfo");
            Should.NotBeNull(setMethod, "setMethod");
            ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
            if (indexParameters.Length == 0)
                return ServiceProvider.ReflectionManager.GetMemberSetter<object>(propertyInfo);
            Func<object, object[], object> @delegate = ServiceProvider
                .ReflectionManager
                .GetMethodDelegate(setMethod);
            object[] indexerValues = GetIndexerValues(path, indexParameters);
            return (o, o1) =>
            {
                var args = new object[indexerValues.Length + 1];
                for (int i = 0; i < indexerValues.Length; i++)
                    args[i] = indexerValues[i];
                args[indexerValues.Length] = o1;
                @delegate(o, args);
            };
        }

        [CanBeNull]
        internal static MemberInfo FindPropertyOrField(this Type type, string memberName, bool staticAccess)
        {
            var flags = MemberFlags.Public | (staticAccess ? MemberFlags.Static : MemberFlags.Instance);
            PropertyInfo property = type.GetPropertyEx(memberName, flags);
            if (property != null)
                return property;
            return type.GetFieldEx(memberName, flags);
        }

        [CanBeNull]
        internal static MethodData FindMethod(this ArgumentData target, string methodName, Type[] typeArgs,
            IList<ArgumentData> args, IEnumerable<Type> knownTypes, bool staticAccess)
        {
            var type = target.Type;
            const MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance;
            var methods = new List<MethodInfo>();
            foreach (var info in type.GetMethodsEx(flags))
            {
                if (info.Name == methodName && info.IsStatic == staticAccess)
                    methods.Add(info);
            }
            if (!staticAccess)
                methods.AddRange(GetExtensionsMethods(methodName, knownTypes));
            return FindBestMethod(target, methods, args, typeArgs);
        }

        [CanBeNull]
        internal static MethodData FindIndexer(this ArgumentData target, IList<ArgumentData> args, bool staticAccess)
        {
            var type = target.Type;
            var methods = new List<MethodInfo>();
            foreach (var property in type.GetPropertiesEx(MemberFlags.Public | MemberFlags.Instance))
            {
                if (property.GetIndexParameters().Length == args.Count)
                {
                    var m = property.GetGetMethod(true);
                    if (m != null && m.IsStatic == staticAccess)
                        methods.Add(m);
                }
            }
            return FindBestMethod(target, methods, args, Empty.Array<Type>());
        }

        internal static object TryParseEnum(string name, Type type)
        {
            if (type.IsEnum())
            {
                foreach (var field in type.GetFieldsEx(MemberFlags.Public | MemberFlags.Static))
                {
                    if (field.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        return field.GetValue(null);
                }
            }
            return null;
        }

        internal static bool IsCompatibleWith(this Type source, Type target)
        {
            if (source == target) return true;
            if (!target.IsValueType()) return target.IsAssignableFrom(source);
            Type st = GetNonNullableType(source);
            Type tt = GetNonNullableType(target);
            if (st != source && tt.Equals(st)) return false;
            TypeCode sc = st.IsEnum() ? TypeCode.Object : st.GetTypeCode();
            TypeCode tc = tt.IsEnum() ? TypeCode.Object : tt.GetTypeCode();
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
                    switch (tc)
                    {
                        case TypeCode.UInt16:
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
                default:
                    if (st == tt) return true;
                    break;
            }
            return false;
        }

        internal static bool IsExtensionMethod(this MethodInfo method)
        {
            return method.IsStatic && method.IsDefined(typeof(ExtensionAttribute), false) &&
                   method.GetParameters().Length > 0;
        }

        internal static IList<ArgumentData> GetMethodArgs(bool isExtensionMethod, ArgumentData target, IList<ArgumentData> args)
        {
            if (!isExtensionMethod)
                return args;
            var actualArgs = new List<ArgumentData> { target };
            actualArgs.AddRange(args);
            return actualArgs;
        }

        private static Type GetNonNullableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        private static bool IsNullableType(this Type type)
        {
            return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static MethodData FindBestMethod(ArgumentData target, IList<MethodInfo> methods, IList<ArgumentData> args, Type[] typeArgs)
        {
            var candidates = new List<MethodData>();
            for (int index = 0; index < methods.Count; index++)
            {
                var methodData = TryInferMethod(methods[index], target, args, typeArgs);
                if (methodData != null)
                    candidates.Add(methodData);
                if (methodData != null && IsCompatible(methodData, GetMethodArgs(methodData.IsExtensionMethod, target, args)))
                    return methodData;
            }
            if (args.Any(data => data.IsLambda))
                return null;
            foreach (var candidate in candidates)
            {
                if (candidate.Parameters.Count == args.Count)
                    return candidate;
            }
            return null;
        }


        private static MethodData TryInferMethod(MethodInfo method, ArgumentData target, IList<ArgumentData> args, Type[] typeArgs)
        {
            var m = ApplyTypeArgs(method, typeArgs);
            if (m != null)
                return new MethodData(m);
            if (!method.IsGenericMethod || typeArgs.Length != 0)
                return null;
            return TryInferGenericMethod(method, GetMethodArgs(method.IsExtensionMethod(), target, args));
        }

        private static MethodData TryInferGenericMethod(MethodInfo method, IList<ArgumentData> args)
        {
            bool hasUnresolved;
            var genericMethod = TryInferGenericMethod(method, args, out hasUnresolved);
            if (genericMethod == null)
                return null;
            if (hasUnresolved)
                return new MethodData(genericMethod, list =>
                {
                    bool unresolved;
                    var m = TryInferGenericMethod(method, list, out unresolved);
                    if (unresolved)
                        throw BindingExceptionManager.InvalidBindingMember(genericMethod.DeclaringType, method.Name);
                    return m;
                });
            return new MethodData(genericMethod);
        }

        private static MethodInfo TryInferGenericMethod(MethodInfo method, IList<ArgumentData> args, out bool hasUnresolved)
        {
            hasUnresolved = false;
            var parameters = method.GetParameters();
            if (args.Count != parameters.Length)
                return null;

            var genericArguments = method.GetGenericArguments();
            var inferredTypes = new Type[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                var argument = genericArguments[i];
                Type inferred = null;
                for (int index = 0; index < parameters.Length; index++)
                {
                    var parameter = parameters[index];
                    var arg = args[index];
                    if (arg.Type != null)
                    {
                        inferred = TryInferParameter(parameter.ParameterType, argument, arg.Type);
                        if (inferred != null)
                            break;
                    }
                }
                if (inferred == null)
                {
                    inferred = argument;
                    hasUnresolved = true;
                }
                inferredTypes[i] = inferred ?? argument;
            }
            return method.MakeGenericMethod(inferredTypes);
        }

        private static bool IsCompatible(this MethodData m, IList<ArgumentData> args)
        {
            var parameters = m.Parameters;
            ParameterInfo parameter = null;
            if (m.IsExtensionMethod)
            {
                if (parameters.Count == 2)
                    parameter = parameters[1];
            }
            else
            {
                if (parameters.Count == 1)
                    parameter = parameters[0];
            }
            if (parameter != null && parameter.IsDefined(typeof(ParamArrayAttribute), true))
                return true;
            if (parameters.Count != args.Count)
                return false;
            for (int index = 0; index < args.Count; ++index)
            {
                var data = args[index];
                if (!IsCompatible(parameters[index].ParameterType, data.Node, data.Expression))
                    return false;
            }
            return true;
        }

        private static Type TryInferParameter(Type source, Type argumentType, Type inputType)
        {
            if (source == argumentType)
                return inputType;
            if (source.IsArray)
                return inputType.IsArray ? inputType.GetElementType() : null;

            if (source.IsGenericType())
            {
                inputType = FindCommonType(source.GetGenericTypeDefinition(), inputType);
                if (inputType == null)
                    return null;

                var srcArgs = source.GetGenericArguments();
                var inputArgs = inputType.GetGenericArguments();
                for (int index = 0; index < srcArgs.Length; index++)
                {
                    var parameter = TryInferParameter(srcArgs[index], argumentType, inputArgs[index]);
                    if (parameter != null)
                        return parameter;
                }
            }
            return null;
        }

        private static MethodInfo ApplyTypeArgs(MethodInfo m, Type[] typeArgs)
        {
            if (typeArgs == null || typeArgs.Length == 0)
            {
                if (!m.IsGenericMethodDefinition)
                    return m;
            }
            else if (m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArgs.Length)
                return m.MakeGenericMethod(typeArgs);
            return null;
        }

        private static bool IsCompatible(Type parameterType, IExpressionNode node, Expression expression)
        {
            var lambdaExpressionNode = node as ILambdaExpressionNode;
            if (lambdaExpressionNode == null)
            {
                Type source = expression.Type;
                if (parameterType.IsByRef)
                    parameterType = parameterType.GetElementType();
                if (!IsCompatibleWith(source, parameterType))
                    return false;
            }
            else
            {
                if (typeof(Expression).IsAssignableFrom(parameterType) && parameterType.IsGenericType())
                    parameterType = parameterType.GetGenericArguments()[0];
                if (!typeof(Delegate).IsAssignableFrom(parameterType))
                    return false;

                var method = parameterType.GetMethodEx("Invoke", MemberFlags.Public | MemberFlags.Instance);
                if (method == null || method.GetParameters().Length != lambdaExpressionNode.Parameters.Count)
                    return false;
            }
            return true;
        }


        internal static Type FindCommonType(Type genericDefinition, Type type)
        {
            foreach (var baseType in SelfAndBaseTypes(type))
            {
                if (baseType.IsGenericType() && baseType.GetGenericTypeDefinition() == genericDefinition)
                    return baseType;
            }
            return null;
        }

        internal static ICollection<Type> SelfAndBaseTypes(Type type)
        {
            var types = new HashSet<Type>(SelfAndBaseClasses(type));
            AddInterface(types, type, true);
            return types;
        }

        private static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
#if PCL_WINRT
                type = type.GetTypeInfo().BaseType;
#else
                type = type.BaseType;
#endif
            }
        }

        private static void AddInterface(HashSet<Type> types, Type type, bool isFirstCall)
        {
            if (!isFirstCall && type.IsInterface() && types.Contains(type))
                return;
            types.Add(type);
            foreach (Type t in type.GetInterfaces())
                AddInterface(types, t, false);
        }

        internal static object[] GetIndexerValues(string path, IList<ParameterInfo> parameters = null, Type castType = null)
        {
            string replace = path.Replace("[", string.Empty).Replace("]", string.Empty);
            var strings = replace.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var result = new object[strings.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                var s = strings[i];
                if (parameters != null)
                    castType = parameters[i].ParameterType;
                result[i] = s == "null" ? null : BindingServiceProvider.ValueConverter(castType, s);
            }
            return result;
        }

        private static IList<MethodInfo> GetExtensionsMethods(string methodName, IEnumerable<Type> knownTypes)
        {
            var list = new List<MethodInfo>();
            lock (TypeToExtensionMethods)
            {
                foreach (var knownType in knownTypes)
                {
                    List<MethodInfo> methods;
                    if (!TypeToExtensionMethods.TryGetValue(knownType, out methods))
                    {
                        methods = knownType
                            .GetMethodsEx()
                            .Where(IsExtensionMethodDelegate)
                            .ToList();
                        TypeToExtensionMethods[knownType] = methods;
                    }
                    for (int index = 0; index < methods.Count; index++)
                    {
                        var method = methods[index];
                        if (method.Name == methodName)
                            list.Add(method);
                    }
                }
            }
            return list;
        }

#if PCL_WINRT
        private static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCode.Empty;
            TypeCode result;
            if (!TypeCodeTable.TryGetValue(type, out result))
                result = TypeCode.Object;
            return result;
        }

        private static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        private static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        private static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        private static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }
#else
        private static TypeCode GetTypeCode(this Type type)
        {
            return Type.GetTypeCode(type);
        }

        private static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        private static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }

        private static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        private static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }
#endif
        #endregion
    }
}