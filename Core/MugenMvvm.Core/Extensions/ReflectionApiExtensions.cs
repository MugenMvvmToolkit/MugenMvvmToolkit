using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces;
using MugenMvvm.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class ReflectionApiExtensions
    {
        #region Properties

        public static Func<Type, MemberFlags, IEnumerable<FieldInfo>> GetFields { get; set; }

        public static Func<Type, string, MemberFlags, FieldInfo> GetField { get; set; }

        public static Func<Type, string, MemberFlags, PropertyInfo> GetProperty { get; set; }

        public static Func<Type, MemberFlags, IEnumerable<MethodInfo>> GetMethods { get; set; }

        public static Func<Type, Type, bool, bool> IsDefined { get; set; }

        public static Func<Type, bool> IsClass { get; set; }

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

        public static IEnumerable<FieldInfo> GetFieldsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetFields(type, flags);
        }

        public static IEnumerable<MethodInfo> GetMethodsUnified(this Type type, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            return GetMethods(type, flags);
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

        public static T GetValueEx<T>(this MemberInfo member, object? target)
        {
            throw new NotImplementedException();
        }

        public static TDelegate GetMethodDelegate<TDelegate>(this IReflectionManager reflectionManager, MethodInfo method) where TDelegate : Delegate
        {
            Should.NotBeNull(method, nameof(method));
            return (TDelegate) reflectionManager.GetMethodDelegate(typeof(TDelegate), method);
        }

        public static bool IsAnonymousClass(this Type type)
        {
            return type.IsDefinedUnified(typeof(CompilerGeneratedAttribute), false) && type.IsClassUnified();
        }

        #endregion
    }
}