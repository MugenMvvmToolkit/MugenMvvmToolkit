using System.Reflection;
using MugenMvvm.Enums;

namespace MugenMvvm.UnitTest
{
    public static class UnifiedReflectionApiInitializer
    {
        #region Methods

        public static void Initialize()
        {
            ReflectionApiExtensions.GetField = (type, s, flags) => type.GetField(s, flags.ToBindingFlags(false));
            ReflectionApiExtensions.GetFields = (type, flags) => type.GetFields(flags.ToBindingFlags(false));
            ReflectionApiExtensions.GetProperty = (type, s, flags) => type.GetProperty(s, flags.ToBindingFlags(false));
            ReflectionApiExtensions.GetMethod = (type, s, flags) => type.GetMethod(s, flags.ToBindingFlags(false));
            ReflectionApiExtensions.GetMethods = (type, flags) => type.GetMethods(flags.ToBindingFlags(false));
            ReflectionApiExtensions.IsAssignableFrom = (type, type1) => type.IsAssignableFrom(type1);
            ReflectionApiExtensions.IsDefined = (type, type1, arg3) => type.IsDefined(type1, arg3);
            ReflectionApiExtensions.IsClass = type => type.IsClass;
            ReflectionApiExtensions.IsValueType = type => type.IsValueType;
            ReflectionApiExtensions.GetGenericArguments = type => type.GenericTypeArguments;
            ReflectionApiExtensions.GetInterfaces = type => type.GetInterfaces();
            ReflectionApiExtensions.IsGenericType = type => type.IsGenericType;
            ReflectionApiExtensions.GetAssembly = type => type.Assembly;
        }

        private static BindingFlags ToBindingFlags(this MemberFlags flags, bool flatten)
        {
            BindingFlags result = default;
            if (flags.HasMemberFlag(MemberFlags.Instance))
                result |= BindingFlags.Instance;
            if (flags.HasMemberFlag(MemberFlags.Static))
                result |= BindingFlags.Static;
            if (flags.HasMemberFlag(MemberFlags.NonPublic))
                result |= BindingFlags.NonPublic;
            if (flags.HasMemberFlag(MemberFlags.Public))
                result |= BindingFlags.Public;
            if (flatten)
                result |= BindingFlags.FlattenHierarchy;
            return result;
        }

        #endregion
    }
}