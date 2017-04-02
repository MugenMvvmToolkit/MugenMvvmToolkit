#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsDataBindingExtensions.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding
{
    public static class XamarinFormsDataBindingExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, IBindingMemberInfo> TypeToContentMember;

        #endregion

        #region Constructors

        static XamarinFormsDataBindingExtensions()
        {
            TypeToContentMember = new Dictionary<Type, IBindingMemberInfo>();
        }

        #endregion

        #region Methods

        public static object Convert(IBindingMemberInfo member, Type type, object value)
        {
            if (XamarinFormsToolkitExtensions.ValueConverter != null)
                return XamarinFormsToolkitExtensions.ValueConverter(member.Member as MemberInfo, type, value);
            if (value == null)
                return type.GetDefaultValue();
            if (type.IsInstanceOfType(value))
                return value;
            if (BindingExtensions.IsConvertible(value))
                return System.Convert.ChangeType(value, type.GetNonNullableType(), BindingServiceProvider.BindingCultureInfo());
            if (type.GetTypeInfo().IsEnum)
            {
                var s = value as string;
                if (s != null)
                    return Enum.Parse(type, s, false);

                return Enum.ToObject(type, value);
            }

            if (type == typeof(string))
                return value.ToString();
            return value;
        }

        public static void ClearBindingsRecursively([CanBeNull] this BindableObject item, bool clearDataContext, bool clearAttachedValues)
        {
            if (item == null)
                return;
            var contentMember = GetContentMember(item.GetType());
            if (contentMember != null)
            {
                var content = contentMember.GetValue(item, null);
                if (!(content is string))
                {
                    var enumerable = content as IEnumerable;
                    if (enumerable == null)
                        ClearBindingsRecursively(content as BindableObject, clearDataContext, clearAttachedValues);
                    else
                    {
                        foreach (var child in enumerable)
                        {
                            var bindableObject = child as BindableObject;
                            if (child == null || bindableObject == null)
                                break;
                            bindableObject.ClearBindingsRecursively(clearDataContext, clearAttachedValues);
                        }
                    }
                }
            }
            item.ClearBindings(clearDataContext, clearAttachedValues);
        }

        private static IBindingMemberInfo GetContentMember(Type type)
        {
            lock (TypeToContentMember)
            {
                IBindingMemberInfo info;
                if (!TypeToContentMember.TryGetValue(type, out info))
                {
                    var attribute = type
                        .GetTypeInfo()
                        .GetCustomAttribute<ContentPropertyAttribute>(true);
                    if (attribute != null)
                    {
                        info = BindingServiceProvider
                            .MemberProvider
                            .GetBindingMember(type, attribute.Name, true, false);
                    }
                    TypeToContentMember[type] = info;
                }
                return info;
            }
        }

        private static Type GetNonNullableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        private static bool IsNullableType(this Type type)
        {
            return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        #endregion
    }
}