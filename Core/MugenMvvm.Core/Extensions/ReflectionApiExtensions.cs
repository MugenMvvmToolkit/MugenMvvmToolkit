using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class ReflectionApiExtensions
    {
        #region Properties

        public static Func<Type, string, MemberFlags, FieldInfo?> GetField { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<FieldInfo>> GetFields { get; set; }

        public static Func<Type, string, MemberFlags, PropertyInfo?> GetProperty { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<PropertyInfo>> GetProperties { get; set; }

        public static Func<Type, string, MemberFlags, MethodInfo?> GetMethod { get; set; }

        public static Func<Type, string, MemberFlags, Type[], MethodInfo?> GetMethodWithTypes { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<MethodInfo>> GetMethods { get; set; }

        public static Func<Type, MemberFlags, Type[], ConstructorInfo?> GetConstructor { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<ConstructorInfo>> GetConstructors { get; set; }

        public static Func<Type, Type, bool> IsAssignableFrom { get; set; }

        public static Func<Type, object, bool> IsInstanceOfType { get; set; }

        public static Func<Type, Type, bool, bool> IsDefined { get; set; }

        public static Func<Type, Assembly> GetAssembly { get; set; }

        public static Func<Type, IEnumerable<Type>> GetInterfaces { get; set; }

        public static Func<Type, Type, bool, IEnumerable<Attribute>> GetCustomAttributes { get; set; }

        public static Func<Type, Type> GetBaseType { get; set; }

        public static Func<Type, IReadOnlyList<Type>> GetGenericArguments { get; set; }

        public static Func<Type, bool> IsClass { get; set; }

        public static Func<Type, bool> IsValueType { get; set; }

        public static Func<Type, bool> IsGenericType { get; set; }

        public static Func<Type, bool> IsInterface { get; set; }

        public static Func<Type, bool> IsAbstract { get; set; }

        public static Func<Type, bool> IsSerializable { get; set; }

        public static Func<Type, bool> IsGenericTypeDefinition { get; set; }

        public static Func<Type, bool> ContainsGenericParameters { get; set; }

        public static Func<PropertyInfo, bool, MethodInfo?> GetGetMethod { get; set; }

        public static Func<PropertyInfo, bool, MethodInfo?> GetSetMethod { get; set; }

        public static Func<Assembly, IList<Type>> GetTypes { get; set; }

        public static Func<Assembly, AssemblyName> GetAssemblyName { get; set; }

        #endregion

        #region Methods

        public static FieldInfo? GetFieldUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetField(type, name, flags);
        }

        public static PropertyInfo? GetPropertyUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetProperty(type, name, flags);
        }

        public static MethodInfo? GetMethodUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetMethod(type, name, flags);
        }

        public static MethodInfo? GetMethodUnified(this Type type, string name, MemberFlags flags, params Type[] types)
        {
            Should.NotBeNull(type, nameof(type));
            return GetMethodWithTypes(type, name, flags, types);
        }

        public static IEnumerable<FieldInfo> GetFieldsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetFields(type, flags);
        }

        public static IEnumerable<PropertyInfo> GetPropertiesUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetProperties(type, flags);
        }

        public static IEnumerable<MethodInfo> GetMethodsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetMethods(type, flags);
        }

        public static ConstructorInfo? GetConstructorUnified(this Type type, MemberFlags flags, Type[] types)
        {
            Should.NotBeNull(type, nameof(type));
            return GetConstructor(type, flags, types);
        }

        public static IEnumerable<ConstructorInfo> GetConstructorsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetConstructors(type, flags);
        }

        public static IEnumerable<Attribute> GetCustomAttributesUnified(this Type type, Type attributeType, bool inherit)
        {
            Should.NotBeNull(type, nameof(type));
            return GetCustomAttributes(type, attributeType, inherit);
        }

        public static Type GetBaseTypeUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return GetBaseType(type);
        }

        public static IEnumerable<Type> GetInterfacesUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return GetInterfaces(type);
        }

        public static IReadOnlyList<Type> GetGenericArgumentsUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return GetGenericArguments(type);
        }

        public static Assembly GetAssemblyUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return GetAssembly(type);
        }

        public static bool IsAssignableFromUnified(this Type type, Type typeFrom)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(typeFrom, nameof(typeFrom));
            return IsAssignableFrom(type, typeFrom);
        }

        public static bool IsInstanceOfTypeUnified(this Type type, object item)
        {
            Should.NotBeNull(type, nameof(type));
            return IsInstanceOfType(type, item);
        }

        public static bool IsDefinedUnified(this Type type, Type attributeType, bool inherit)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(attributeType, nameof(attributeType));
            return IsDefined(type, attributeType, inherit);
        }

        public static bool IsClassUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return IsClass(type);
        }

        public static bool IsAbstractUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return IsAbstract(type);
        }

        public static bool IsSerializableUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return IsSerializable(type);
        }

        public static bool IsInterfaceUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return IsInterface(type);
        }

        public static bool IsValueTypeUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return IsValueType(type);
        }

        public static bool IsGenericTypeUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return IsGenericType(type);
        }

        public static bool IsGenericTypeDefinitionUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return IsGenericTypeDefinition(type);
        }

        public static bool ContainsGenericParametersUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return ContainsGenericParameters(type);
        }

        public static MethodInfo? GetGetMethodUnified(this PropertyInfo propertyInfo, bool nonPublic)
        {
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            return GetGetMethod(propertyInfo, nonPublic);
        }

        public static MethodInfo? GetSetMethodUnified(this PropertyInfo propertyInfo, bool nonPublic)
        {
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            return GetSetMethod(propertyInfo, nonPublic);
        }

        public static bool IsStatic(this MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
            {
                var method = propertyInfo.CanRead
                    ? propertyInfo.GetGetMethodUnified(true)
                    : propertyInfo.GetSetMethodUnified(true);
                return method != null && method.IsStatic;
            }

            if (member is MethodInfo methodInfo)
                return methodInfo.IsStatic;
            return member is FieldInfo fieldInfo && fieldInfo.IsStatic;
        }

        public static bool IsAnonymousClass(this Type type)
        {
            return type.IsDefinedUnified(typeof(CompilerGeneratedAttribute), false) && type.IsClassUnified();
        }

        public static IList<Type> GetTypesUnified(this Assembly assembly, bool throwOnError)
        {
            Should.NotBeNull(assembly, nameof(assembly));
            try
            {
                return GetTypes(assembly);
            }
            catch (ReflectionTypeLoadException e) when (!throwOnError)
            {
                if (Tracer.TraceError)
                    Tracer.Error(e.Flatten(true));
            }

            return Default.EmptyArray<Type>();
        }

        public static AssemblyName GetAssemblyNameUnified(this Assembly assembly)
        {
            Should.NotBeNull(assembly, nameof(assembly));
            return GetAssemblyName(assembly);
        }

        #endregion
    }
}