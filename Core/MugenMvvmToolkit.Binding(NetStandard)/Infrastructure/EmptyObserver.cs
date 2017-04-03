#region Copyright

// ****************************************************************************
// <copyright file="EmptyObserver.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public sealed class EmptyObserver : IObserver, IBindingPathMembers
    {
        #region Fields

        public static readonly EmptyObserver Instance = new EmptyObserver();

        #endregion

        #region Constructors

        private EmptyObserver()
        {
        }

        #endregion

        #region Properties

        bool IObserver.IsAlive => true;

        IBindingPath IObserver.Path => BindingPath.Empty;

        object IObserver.Source => null;

        IBindingPath IBindingPathMembers.Path => BindingPath.Empty;

        bool IBindingPathMembers.AllMembersAvailable => true;

        IList<IBindingMemberInfo> IBindingPathMembers.Members => new[] { BindingMemberInfo.Empty };

        IBindingMemberInfo IBindingPathMembers.LastMember => BindingMemberInfo.Empty;

        object IBindingPathMembers.Source => null;

        object IBindingPathMembers.PenultimateValue => null;

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose()
        {
        }

        void IObserver.Update()
        {
        }

        bool IObserver.Validate(bool throwOnError)
        {
            return true;
        }

        object IObserver.GetActualSource(bool throwOnError)
        {
            return null;
        }

        IBindingPathMembers IObserver.GetPathMembers(bool throwOnError)
        {
            return this;
        }

        event EventHandler<IObserver, ValueChangedEventArgs> IObserver.ValueChanged
        {
            add { }
            remove { }
        }

        #endregion
    }
}