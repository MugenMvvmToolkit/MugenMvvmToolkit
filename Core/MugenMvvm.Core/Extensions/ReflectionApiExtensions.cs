using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Infrastructure;
using MugenMvvm.Interfaces;
using MugenMvvm.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class ReflectionApiExtensions
    {
        #region Properties

        public static Func<Type, string, MemberFlags, FieldInfo> GetField { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<FieldInfo>> GetFields { get; set; }

        public static Func<Type, string, MemberFlags, PropertyInfo> GetProperty { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<PropertyInfo>> GetProperties { get; set; }

        public static Func<Type, string, MemberFlags, MethodInfo> GetMethod { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<MethodInfo>> GetMethods { get; set; }

        public static Func<Type, Type, bool> IsAssignableFrom { get; set; }

        public static Func<Type, Type, bool, bool> IsDefined { get; set; }

        public static Func<Type, Assembly> GetAssembly { get; set; }

        public static Func<Type, Type, bool, IEnumerable<Attribute>> GetCustomAttributes { get; set; }

        public static Func<Type, Type> GetBaseType { get; set; }

        public static Func<Type, bool> IsClass { get; set; }

        public static Func<Type, bool> IsValueType { get; set; }

        public static Func<Type, bool> IsGenericType { get; set; }

        public static Func<Type, bool> IsInterface { get; set; }

        public static Func<Type, bool> IsAbstract { get; set; }

        public static Func<Type, bool> IsGenericTypeDefinition { get; set; }

        public static Func<PropertyInfo, bool, MethodInfo> GetGetMethod { get; set; }

        public static Func<PropertyInfo, bool, MethodInfo> GetSetMethod { get; set; }

        public static Func<Assembly, IList<Type>> GetTypes { get; set; }

        public static Func<Assembly, AssemblyName> GetAssemblyName { get; set; }

        #endregion

        #region Methods

        public static FieldInfo GetFieldUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetField(type, name, flags);
        }

        public static PropertyInfo GetPropertyUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetProperty(type, name, flags);
        }

        public static MethodInfo GetMethodUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetMethod(type, name, flags);
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

        public static MethodInfo GetGetMethodUnified(this PropertyInfo propertyInfo, bool nonPublic)
        {
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            return GetGetMethod(propertyInfo, nonPublic);
        }

        public static MethodInfo GetSetMethodUnified(this PropertyInfo propertyInfo, bool nonPublic)
        {
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            return GetSetMethod(propertyInfo, nonPublic);
        }

        public static T GetValueEx<T>(this MemberInfo member, object? target)
        {
            return Singleton<IReflectionManager>.Instance.GetMemberGetter<T>(member).Invoke(target);
        }

        public static bool IsStatic(this MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
            {
                MethodInfo method = propertyInfo.CanRead
                    ? propertyInfo.GetGetMethodUnified(true)
                    : propertyInfo.GetSetMethodUnified(true);
                return method != null && method.IsStatic;
            }

            if (member is MethodInfo methodInfo)
                return methodInfo.IsStatic;
            return member is FieldInfo fieldInfo && fieldInfo.IsStatic;
        }

        public static TDelegate GetMethodDelegate<TDelegate>(this IReflectionManager reflectionManager, MethodInfo method) where TDelegate : Delegate
        {
            Should.NotBeNull(method, nameof(method));
            return (TDelegate)reflectionManager.GetMethodDelegate(typeof(TDelegate), method);
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

        [Pure]
        public static bool HasMemberFlag(this MemberFlags es, MemberFlags value)
        {
            return (es & value) == value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsEx(this Type x, Type y)//note idk why but default implementation doesn't use ReferenceEquals before equals check
        {
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsEx(this MemberInfo x, MemberInfo y)
        {
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        #endregion
    }
}