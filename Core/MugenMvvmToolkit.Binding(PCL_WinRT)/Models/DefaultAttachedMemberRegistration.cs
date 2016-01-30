#region Copyright

// ****************************************************************************
// <copyright file="DefaultAttachedMemberRegistration.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public sealed class DefaultAttachedMemberRegistration<TType>
    {
        #region Fields

        private readonly IBindingMemberInfo _defaultMember;
        private readonly string _path;

        #endregion

        #region Constructors

        public DefaultAttachedMemberRegistration([NotNull] IBindingMemberInfo defaultMember, string path = null)
        {
            Should.NotBeNull(defaultMember, nameof(defaultMember));
            _defaultMember = defaultMember;
            _path = path ?? defaultMember.Path;
        }

        #endregion

        #region Methods

        public IAttachedBindingMemberInfo<TTarget, TType> ToAttachedBindingMember<TTarget>(string path = null)
            where TTarget : class
        {
            return AttachedBindingMember.CreateMember<TTarget, TType>(path ?? _path, GetValue, SetValue, Observe);
        }

        public IDisposable Observe(IBindingMemberInfo bindingMemberInfo, object item, IEventListener arg3)
        {
            return GetMember(item).TryObserve(item, arg3);
        }

        public object SetValue(IBindingMemberInfo bindingMemberInfo, object item, object[] arg3)
        {
            return GetMember(item).SetValue(item, arg3);
        }

        public TType GetValue(IBindingMemberInfo bindingMemberInfo, object item, object[] arg3)
        {
            return (TType)GetMember(item).GetValue(item, arg3);
        }

        private IBindingMemberInfo GetMember(object item)
        {
            return BindingServiceProvider
                .MemberProvider
                .GetBindingMember(item.GetType(), _path, true, false) ?? _defaultMember;
        }

        #endregion
    }
}
