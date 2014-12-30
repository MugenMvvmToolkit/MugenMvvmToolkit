#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberProviderEx.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
#else
using System.ComponentModel;
using System.Windows;
#endif
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindingMemberProviderEx : BindingMemberProvider
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberProvider" /> class.
        /// </summary>
        public BindingMemberProviderEx()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberProvider" /> class.
        /// </summary>
        public BindingMemberProviderEx([NotNull] BindingMemberProvider provider)
            : base(provider)
        {
        }

        #endregion

        #region Overrides of BindingMemberProvider

        /// <summary>
        ///     Gets an instance of <see cref="IBindingMemberInfo" /> using the source type and binding path.
        /// </summary>
        /// <param name="sourceType">The specified source type.</param>
        /// <param name="path">The specified binding path.</param>
        /// <returns>The instance of <see cref="IBindingMemberInfo" />.</returns>
        protected override IBindingMemberInfo GetExplicitBindingMember(Type sourceType, string path)
        {
            if (typeof(DependencyObject).IsAssignableFrom(sourceType))
            {
                var property = GetDependencyProperty(sourceType, path);
                if (property != null)
                {
                    IBindingMemberInfo changeEvent = BindingExtensions.TryFindMemberChangeEvent(this, sourceType, path);
#if WPF
                    return new DependencyPropertyBindingMember(property, path, property.PropertyType, property.ReadOnly, sourceType.GetProperty(path), changeEvent);
#else

                    var member = sourceType.GetPropertyEx(path);
                    Type type = typeof(object);
                    bool readOnly = false;
                    if (member != null)
                    {
                        type = member.PropertyType;
                        readOnly = !member.CanWrite;
                    }
                    return new DependencyPropertyBindingMember(property, path, type, readOnly, member, changeEvent);
#endif
                }
            }
            return base.GetExplicitBindingMember(sourceType, path);
        }

        #endregion

        #region Methods

        private static DependencyProperty GetDependencyProperty(Type type, string name)
        {
#if WPF
            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromName(name, type, type);
            if (descriptor == null)
                return null;
            return descriptor.DependencyProperty;
#else

            FieldInfo fieldInfo = type.GetFieldEx(name + "Property", MemberFlags.Public | MemberFlags.Static) ??
                                  type.GetFieldEx(name, MemberFlags.Public | MemberFlags.Static);
            var property = fieldInfo == null
                ? null
                : fieldInfo.GetValue(null) as DependencyProperty;
            if (property == null)
            {
                var prop = type.GetPropertyEx(name + "Property", MemberFlags.Public | MemberFlags.Static) ??
                                  type.GetPropertyEx(name, MemberFlags.Public | MemberFlags.Static);
                property = (prop == null || !prop.CanRead)
                    ? null
                    : prop.GetValue(null, null) as DependencyProperty;
            }
            return property;
#endif
        }

        #endregion
    }
}