using System;
using System.Collections.Generic;
using System.Reflection;
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

        public static T GetValueEx<T>(this MemberInfo member, object? target)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}