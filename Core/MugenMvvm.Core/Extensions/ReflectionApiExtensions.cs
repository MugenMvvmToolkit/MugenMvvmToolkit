using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class ReflectionApiExtensions
    {
        #region Properties

        public static Func<Type, string, MemberFlags, FieldInfo?>? GetField { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<FieldInfo>>? GetFields { get; set; }

        public static Func<Type, string, MemberFlags, PropertyInfo?>? GetProperty { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<PropertyInfo>>? GetProperties { get; set; }

        public static Func<Type, string, MemberFlags, MethodInfo?>? GetMethod { get; set; }

        public static Func<Type, string, MemberFlags, Type[], MethodInfo?>? GetMethodWithTypes { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<MethodInfo>>? GetMethods { get; set; }

        public static Func<Type, string, MemberFlags, EventInfo?>? GetEvent { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<EventInfo>>? GetEvents { get; set; }

        public static Func<Type, MemberFlags, Type[], ConstructorInfo?>? GetConstructor { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<ConstructorInfo>>? GetConstructors { get; set; }

        public static Func<Type, Type, bool>? IsAssignableFrom { get; set; }

        public static Func<Type, object, bool>? IsInstanceOfType { get; set; }

        public static Func<Type, Type, bool, bool>? IsDefined { get; set; }

        public static Func<Type, Assembly>? GetAssembly { get; set; }

        public static Func<Type, IEnumerable<Type>>? GetInterfaces { get; set; }

        public static Func<Type, Type, bool, IEnumerable<Attribute>>? GetCustomAttributes { get; set; }

        public static Func<Type, Type>? GetBaseType { get; set; }

        public static Func<Type, IEnumerable<Type>>? GetGenericArguments { get; set; }

        public static Func<Type, bool>? IsClass { get; set; }

        public static Func<Type, bool>? IsValueType { get; set; }

        public static Func<Type, bool>? IsGenericType { get; set; }

        public static Func<Type, bool>? IsInterface { get; set; }

        public static Func<Type, bool>? IsAbstract { get; set; }

        public static Func<Type, bool>? IsSerializable { get; set; }

        public static Func<Type, bool>? IsGenericTypeDefinition { get; set; }

        public static Func<Type, bool>? ContainsGenericParameters { get; set; }

        public static Func<PropertyInfo, bool, MethodInfo?>? GetGetMethod { get; set; }

        public static Func<PropertyInfo, bool, MethodInfo?>? GetSetMethod { get; set; }

        public static Func<EventInfo, bool, MethodInfo?>? GetAddMethod { get; set; }

        public static Func<EventInfo, bool, MethodInfo?>? GetRemoveMethod { get; set; }

        #endregion

        #region Methods

        public static FieldInfo? GetFieldUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetField == null)
                return TypeInfoReflectionApiExtensions.GetField(type, name, flags);
            return GetField(type, name, flags);
        }

        public static PropertyInfo? GetPropertyUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetProperty == null)
                return TypeInfoReflectionApiExtensions.GetProperty(type, name, flags);
            return GetProperty.Invoke(type, name, flags);
        }

        public static MethodInfo? GetMethodUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetMethod == null)
                return TypeInfoReflectionApiExtensions.GetMethod(type, name, flags);
            return GetMethod(type, name, flags);
        }

        public static MethodInfo? GetMethodUnified(this Type type, string name, MemberFlags flags, params Type[] types)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetMethodWithTypes == null)
                return TypeInfoReflectionApiExtensions.GetMethod(type, name, flags, types);
            return GetMethodWithTypes(type, name, flags, types);
        }

        public static EventInfo? GetEventUnified(this Type type, string name, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetEvent == null)
                return TypeInfoReflectionApiExtensions.GetEvent(type, name, flags);
            return GetEvent(type, name, flags);
        }

        public static ConstructorInfo? GetConstructorUnified(this Type type, MemberFlags flags, Type[] types)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetConstructor == null)
                return TypeInfoReflectionApiExtensions.GetConstructor(type, flags, types);
            return GetConstructor(type, flags, types);
        }

        public static IEnumerable<FieldInfo> GetFieldsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetFields == null)
                return TypeInfoReflectionApiExtensions.GetFields(type, flags);
            return GetFields(type, flags);
        }

        public static IEnumerable<PropertyInfo> GetPropertiesUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetProperties == null)
                return TypeInfoReflectionApiExtensions.GetProperties(type, flags);
            return GetProperties(type, flags);
        }

        public static IEnumerable<MethodInfo> GetMethodsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetMethods == null)
                return TypeInfoReflectionApiExtensions.GetMethods(type, flags);
            return GetMethods(type, flags);
        }

        public static IEnumerable<EventInfo> GetEventsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetEvents == null)
                return TypeInfoReflectionApiExtensions.GetEvents(type, flags);
            return GetEvents(type, flags);
        }

        public static IEnumerable<ConstructorInfo> GetConstructorsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetConstructors == null)
                return TypeInfoReflectionApiExtensions.GetConstructors(type, flags);
            return GetConstructors(type, flags);
        }

        public static IEnumerable<Attribute> GetCustomAttributesUnified(this Type type, Type attributeType, bool inherit)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetCustomAttributes == null)
                return TypeInfoReflectionApiExtensions.GetCustomAttributes(type, attributeType, inherit);
            return GetCustomAttributes(type, attributeType, inherit);
        }

        public static Type GetBaseTypeUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetBaseType == null)
                return TypeInfoReflectionApiExtensions.GetBaseType(type);
            return GetBaseType(type);
        }

        public static IEnumerable<Type> GetInterfacesUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetInterfaces == null)
                return TypeInfoReflectionApiExtensions.GetInterfaces(type);
            return GetInterfaces(type);
        }

        public static IEnumerable<Type> GetGenericArgumentsUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetGenericArguments == null)
                return TypeInfoReflectionApiExtensions.GetGenericArguments(type);
            return GetGenericArguments(type);
        }

        public static Assembly GetAssemblyUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetAssembly == null)
                return TypeInfoReflectionApiExtensions.GetAssembly(type);
            return GetAssembly(type);
        }

        public static bool IsAssignableFromUnified(this Type type, Type typeFrom)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(typeFrom, nameof(typeFrom));
            if (IsAssignableFrom == null)
                return TypeInfoReflectionApiExtensions.IsAssignableFrom(type, typeFrom);
            return IsAssignableFrom(type, typeFrom);
        }

        public static bool IsInstanceOfTypeUnified(this Type type, object item)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsInstanceOfType == null)
                return TypeInfoReflectionApiExtensions.IsInstanceOfType(type, item);
            return IsInstanceOfType(type, item);
        }

        public static bool IsDefinedUnified(this Type type, Type attributeType, bool inherit)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(attributeType, nameof(attributeType));
            if (IsDefined == null)
                return TypeInfoReflectionApiExtensions.IsDefined(type, attributeType, inherit);
            return IsDefined(type, attributeType, inherit);
        }

        public static bool IsClassUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsClass == null)
                return TypeInfoReflectionApiExtensions.IsClass(type);
            return IsClass(type);
        }

        public static bool IsAbstractUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsAbstract == null)
                return TypeInfoReflectionApiExtensions.IsAbstract(type);
            return IsAbstract(type);
        }

        public static bool IsSerializableUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsSerializable == null)
                return TypeInfoReflectionApiExtensions.IsSerializable(type);
            return IsSerializable(type);
        }

        public static bool IsInterfaceUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsInterface == null)
                return TypeInfoReflectionApiExtensions.IsInterface(type);
            return IsInterface(type);
        }

        public static bool IsValueTypeUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsValueType == null)
                return TypeInfoReflectionApiExtensions.IsValueType(type);
            return IsValueType(type);
        }

        public static bool IsGenericTypeUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsGenericType == null)
                return TypeInfoReflectionApiExtensions.IsGenericType(type);
            return IsGenericType(type);
        }

        public static bool IsGenericTypeDefinitionUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (IsGenericTypeDefinition == null)
                return TypeInfoReflectionApiExtensions.IsGenericTypeDefinition(type);
            return IsGenericTypeDefinition(type);
        }

        public static bool ContainsGenericParametersUnified(this Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (ContainsGenericParameters == null)
                return TypeInfoReflectionApiExtensions.ContainsGenericParameters(type);
            return ContainsGenericParameters(type);
        }

        public static MethodInfo? GetGetMethodUnified(this PropertyInfo propertyInfo, bool nonPublic)
        {
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            if (GetGetMethod == null)
                return TypeInfoReflectionApiExtensions.GetGetMethod(propertyInfo, nonPublic);
            return GetGetMethod(propertyInfo, nonPublic);
        }

        public static MethodInfo? GetSetMethodUnified(this PropertyInfo propertyInfo, bool nonPublic)
        {
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            if (GetSetMethod == null)
                return TypeInfoReflectionApiExtensions.GetSetMethod(propertyInfo, nonPublic);
            return GetSetMethod(propertyInfo, nonPublic);
        }

        public static MethodInfo? GetAddMethodUnified(this EventInfo eventInfo, bool nonPublic)
        {
            Should.NotBeNull(eventInfo, nameof(eventInfo));
            if (GetAddMethod == null)
                return TypeInfoReflectionApiExtensions.GetAddMethod(eventInfo, nonPublic);
            return GetAddMethod(eventInfo, nonPublic);
        }

        public static MethodInfo? GetRemoveMethodUnified(this EventInfo eventInfo, bool nonPublic)
        {
            Should.NotBeNull(eventInfo, nameof(eventInfo));
            if (GetRemoveMethod == null)
                return TypeInfoReflectionApiExtensions.GetRemoveMethod(eventInfo, nonPublic);
            return GetRemoveMethod(eventInfo, nonPublic);
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

        #endregion
    }
}