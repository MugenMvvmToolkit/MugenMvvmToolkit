#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberInfoMock.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingMemberInfoMock : IBindingMemberInfo
    {
        #region Constructors

        public BindingMemberInfoMock()
        {
            Type = typeof(object);
            Path = string.Empty;
            MemberType = BindingMemberType.Unset;
        }

        #endregion

        #region Properties

        public Func<object, object[], object> GetValue;

        public Action<object, object[]> SetValue;

        public Func<object, IEventListener, IDisposable> TryObserveMember;

        #endregion

        #region Implementation of IBindingMemberInfo

        public string Path { get; set; }

        public Type Type { get; set; }

        public object Member { get; set; }

        public BindingMemberType MemberType { get; set; }

        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        public bool CanObserve { get; set; }

        object IBindingMemberInfo.GetValue(object source, object[] args)
        {
            return GetValue(source, args);
        }

        object IBindingMemberInfo.SetValue(object source, object[] args)
        {
            SetValue(source, args);
            return null;
        }

        public object SetSingleValue(object source, object value)
        {
            SetValue(source, new[] { value });
            return null;
        }

        IDisposable IBindingMemberInfo.TryObserve(object source, IEventListener listener)
        {
            return TryObserveMember(source, listener);
        }

        #endregion
    }
}
