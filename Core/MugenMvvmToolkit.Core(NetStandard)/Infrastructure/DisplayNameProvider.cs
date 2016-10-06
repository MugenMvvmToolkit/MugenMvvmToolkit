#region Copyright

// ****************************************************************************
// <copyright file="DisplayNameProvider.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Linq;
using System.Reflection;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class DisplayNameProvider : IDisplayNameProvider
    {
        #region Fields

        private const string DisplayNameAttributeName = "DisplayNameAttribute";
        private const string DisplayAttributeName = "DisplayAttribute";
        private const string DisplayNamePropertyName = "DisplayName";
        private const string GetNameMethodName = "GetName";

        private static readonly Dictionary<MemberInfo, Func<string>> MembersToNames;

        #endregion

        #region Constructors

        static DisplayNameProvider()
        {
            MembersToNames = new Dictionary<MemberInfo, Func<string>>();
        }

        #endregion

        #region Implementation of IDisplayNameProvider

        public Func<string> GetDisplayNameAccessor(MemberInfo memberInfo)
        {
            Should.NotBeNull(memberInfo, nameof(memberInfo));
            Func<string> name;
            lock (MembersToNames)
            {
                if (!MembersToNames.TryGetValue(memberInfo, out name))
                {
                    name = GetDisplayNameInternal(memberInfo);
                    MembersToNames[memberInfo] = name;
                }
            }
            return name;
        }

        #endregion

        #region Methods

        protected virtual Func<string> GetDisplayNameInternal(MemberInfo memberInfo)
        {
#if NET_STANDARD
            var typeInfo = memberInfo as TypeInfo;
#else
            var typeInfo = memberInfo as Type;
#endif

            if (typeInfo != null)
            {
#if NET_STANDARD
                var type = typeInfo.AsType();
#else
                var type = typeInfo;
#endif
                var metadataTypes = new List<Type>(DataAnnotationValidatior.GetMetadataTypes(type));
                metadataTypes.Insert(0, type);
                for (int index = 0; index < metadataTypes.Count; index++)
                {
#if NET_STANDARD
                    var result = TryGetDisplayNameAttributeAccessor(metadataTypes[index].GetTypeInfo());
#else
                    var result = TryGetDisplayNameAttributeAccessor(metadataTypes[index]);
#endif

                    if (result != null)
                        return result;
                }
                return () => string.Empty;
            }
            Func<string> accessor = TryGetDisplayAttributeAccessor(memberInfo);
            if (accessor != null)
                return accessor;

            var types = DataAnnotationValidatior.GetMetadataTypes(memberInfo.DeclaringType);
            for (int index = 0; index < types.Length; index++)
            {
                MemberInfo metaMemberInfo = TryFindMetaMemberInfo(types[index], memberInfo);
                if (metaMemberInfo == null)
                    continue;
                accessor = TryGetDisplayAttributeAccessor(metaMemberInfo);
                if (accessor != null)
                    return accessor;
            }
            string s = memberInfo.Name;
            return () => s;
        }

        private static Func<string> TryGetDisplayNameAttributeAccessor(MemberInfo memberInfo)
        {
            foreach (var attr in memberInfo.GetCustomAttributes(typeof(Attribute), true))
            {
                Type attrType = attr.GetType();
                if (attrType.Name != DisplayNameAttributeName)
                    continue;
                PropertyInfo propertyInfo = attrType.GetPropertyEx(DisplayNamePropertyName, MemberFlags.Public | MemberFlags.Instance);
                if (propertyInfo == null || !propertyInfo.PropertyType.Equals(typeof(string)) ||
                    propertyInfo.GetIndexParameters().Length != 0)
                    continue;
                Func<object, string> memberAccess = ServiceProvider.ReflectionManager.GetMemberGetter<string>(propertyInfo);
                var o = attr;
                return () => memberAccess(o);
            }
            return null;
        }

        private static Func<string> TryGetDisplayAttributeAccessor(MemberInfo memberInfo)
        {
            var accessor = TryGetDisplayNameAttributeAccessor(memberInfo);
            if (accessor != null)
                return accessor;
            foreach (var attr in memberInfo.GetCustomAttributes(typeof(Attribute), true))
            {
                Type attrType = attr.GetType();
                if (attrType.Name != DisplayAttributeName)
                    continue;
                MethodInfo methodInfo = attrType.GetMethodEx(GetNameMethodName, MemberFlags.Public | MemberFlags.Instance);
                if (methodInfo == null || !methodInfo.ReturnType.Equals(typeof(string)) ||
                    methodInfo.GetParameters().Length != 0)
                    continue;
                Func<object, object[], object> methodDelegate = ServiceProvider.ReflectionManager.GetMethodDelegate(methodInfo);
                object value = attr;
                return () => (string)methodDelegate(value, Empty.Array<object>());
            }
            return null;
        }

        private static MemberInfo TryFindMetaMemberInfo(Type metaType, MemberInfo member)
        {
            var flags = MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Instance;
            var property = member as PropertyInfo;
            if (property != null)
                return metaType
                    .GetPropertiesEx(flags)
                    .FirstOrDefault(
                        info =>
                            info.Name == property.Name &&
                            info.GetIndexParameters().SequenceEqual(property.GetIndexParameters()));
            var field = member as FieldInfo;
            if (field != null)
                return metaType.GetFieldEx(field.Name, flags);
            var method = member as MethodInfo;
            if (method != null)
                return metaType
                    .GetMethodsEx(flags)
                    .FirstOrDefault(info => info.Name == method.Name && info.ReturnType.Equals(method.ReturnType)
                                            &&
                                            info.GetParameters()
                                                .Select(parameterInfo => parameterInfo.ParameterType)
                                                .SequenceEqual(method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType)));
            var eventInfo = member as EventInfo;
            if (eventInfo != null)
#if NET_STANDARD
                return metaType.GetRuntimeEvents().FirstOrDefault(info => info.Name == eventInfo.Name);
#else
                return metaType.GetEvents().FirstOrDefault(info => info.Name == eventInfo.Name);
#endif

            return null;
        }

        #endregion
    }
}
