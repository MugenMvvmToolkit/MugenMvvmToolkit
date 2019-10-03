using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Enums;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class TypeInfoReflectionApiExtensions
    {
        #region Methods

        public static FieldInfo? GetField(Type type, string name, MemberFlags flags)
        {
            foreach (var field in GetFields(type, flags))
            {
                if (field.Name == name)
                    return field;
            }

            return null;
        }

        public static PropertyInfo? GetProperty(Type type, string name, MemberFlags flags)
        {
            foreach (var property in GetProperties(type, flags))
            {
                if (property.Name == name)
                    return property;
            }

            return null;
        }

        public static MethodInfo? GetMethod(Type type, string name, MemberFlags flags)
        {
            MethodInfo? result = null;
            foreach (var method in GetMethods(type, flags))
            {
                if (method.Name == name)
                {
                    if (result != null)
                        ThrowAmbiguousMatchException();
                    result = method;
                }
            }

            return result;
        }

        public static MethodInfo? GetMethod(Type type, string name, MemberFlags flags, Type[] types)
        {
            MethodInfo? result = null;
            foreach (var method in GetMethods(type, flags))
            {
                if (method.Name == name && FilterMethod(method, types))
                {
                    if (result != null)
                        ThrowAmbiguousMatchException();
                    result = method;
                }
            }

            return result;
        }


        public static EventInfo? GetEvent(Type type, string name, MemberFlags flags)
        {
            foreach (var eventInfo in GetEvents(type, flags))
            {
                if (eventInfo.Name == name)
                    return eventInfo;
            }

            return null;
        }

        public static ConstructorInfo? GetConstructor(Type type, MemberFlags flags, Type[] types)
        {
            foreach (var constructorInfo in GetConstructors(type, flags))
            {
                if (FilterMethod(constructorInfo, types))
                    return constructorInfo;
            }

            return null;
        }

        public static IEnumerable<FieldInfo> GetFields(Type type, MemberFlags flags)
        {
            foreach (var field in type.GetRuntimeFields())
            {
                if (FilterField(field, flags))
                    yield return field;
            }
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type type, MemberFlags flags)
        {
            foreach (var property in type.GetRuntimeProperties())
            {
                if (FilterProperty(property, flags))
                    yield return property;
            }
        }

        public static IEnumerable<EventInfo> GetEvents(Type type, MemberFlags flags)
        {
            foreach (var eventInfo in type.GetRuntimeEvents())
            {
                if (FilterEvent(eventInfo, flags))
                    yield return eventInfo;
            }
        }

        public static IEnumerable<MethodInfo> GetMethods(Type type, MemberFlags flags)
        {
            foreach (var method in type.GetRuntimeMethods())
            {
                if (FilterMethod(method, flags))
                    yield return method;
            }
        }

        public static IEnumerable<ConstructorInfo> GetConstructors(Type type, MemberFlags flags)
        {
            foreach (var constructor in type.GetTypeInfo().DeclaredConstructors)
            {
                if (FilterMethod(constructor, flags))
                    yield return constructor;
            }
        }

        public static IEnumerable<Attribute> GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);
        }

        public static Type GetBaseType(Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static IEnumerable<Type> GetInterfaces(Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static IEnumerable<Type> GetGenericArguments(Type type)
        {
            return type.GenericTypeArguments;
        }

        public static Assembly GetAssembly(Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static bool IsAssignableFrom(Type type, Type typeFrom)
        {
            return type.GetTypeInfo().IsAssignableFrom(typeFrom.GetTypeInfo());
        }

        public static bool IsInstanceOfType(Type type, object? item)
        {
            if (item == null)
                return false;
            return IsAssignableFrom(type, item.GetType());
        }

        public static bool IsDefined(Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().IsDefined(attributeType, inherit);
        }

        public static bool IsClass(Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsAbstract(Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsSerializable(Type type)
        {
            return type.GetTypeInfo().IsSerializable;
        }

        public static bool IsInterface(Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsValueType(Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsEnum(Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsGenericType(Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool ContainsGenericParameters(Type type)
        {
            return type.GetTypeInfo().ContainsGenericParameters;
        }

        public static MethodInfo? GetGetMethod(PropertyInfo propertyInfo, bool nonPublic)
        {
            var method = propertyInfo.GetMethod;
            if (nonPublic)
                return method;
            return method.IsPublic ? method : null;
        }

        public static MethodInfo? GetSetMethod(PropertyInfo propertyInfo, bool nonPublic)
        {
            var method = propertyInfo.SetMethod;
            if (nonPublic)
                return method;
            return method.IsPublic ? method : null;
        }

        public static MethodInfo? GetAddMethod(EventInfo eventInfo, bool nonPublic)
        {
            var method = eventInfo.AddMethod;
            if (nonPublic)
                return method;
            return method.IsPublic ? method : null;
        }

        public static MethodInfo? GetRemoveMethod(EventInfo eventInfo, bool nonPublic)
        {
            var method = eventInfo.RemoveMethod;
            if (nonPublic)
                return method;
            return method.IsPublic ? method : null;
        }

        private static bool FilterProperty(PropertyInfo property, MemberFlags flags)
        {
            if (property == null)
                return false;
            if (property.CanRead && FilterMethod(property.GetMethod, flags))
                return true;
            return property.CanWrite && FilterMethod(property.SetMethod, flags);
        }

        private static bool FilterEvent(EventInfo eventInfo, MemberFlags flags)
        {
            if (eventInfo == null)
                return false;
            var m = eventInfo.AddMethod ?? eventInfo.RemoveMethod;
            return m != null && FilterMethod(m, flags);
        }

        private static bool FilterField(FieldInfo field, MemberFlags flags)
        {
            if (field == null)
                return false;
            return (flags.HasFlag(MemberFlags.Static) && field.IsStatic ||
                    flags.HasFlag(MemberFlags.Instance) && !field.IsStatic) &&
                   (flags.HasFlag(MemberFlags.NonPublic) && !field.IsPublic ||
                    flags.HasFlag(MemberFlags.Public) && field.IsPublic);
        }

        private static bool FilterMethod(MethodBase method, MemberFlags flags)
        {
            if (method == null)
                return false;
            return (flags.HasFlag(MemberFlags.Static) && method.IsStatic ||
                    flags.HasFlag(MemberFlags.Instance) && !method.IsStatic) &&
                   (flags.HasFlag(MemberFlags.NonPublic) && !method.IsPublic ||
                    flags.HasFlag(MemberFlags.Public) && method.IsPublic);
        }

        private static bool FilterMethod(MethodBase method, Type[] types)
        {
            var parameterInfos = method.GetParameters();
            if (parameterInfos.Length != types.Length)
                return false;
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                if (!parameterInfos[i].ParameterType.EqualsEx(types[i]))
                    return false;
            }

            return true;
        }

        private static void ThrowAmbiguousMatchException()
        {
            throw new AmbiguousMatchException();
        }

        #endregion
    }
}