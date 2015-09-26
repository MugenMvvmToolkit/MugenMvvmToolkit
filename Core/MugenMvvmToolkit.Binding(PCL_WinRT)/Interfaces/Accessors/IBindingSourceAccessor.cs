#region Copyright

// ****************************************************************************
// <copyright file="IBindingSourceAccessor.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Accessors
{
    public interface IBindingSourceAccessor : IDisposable
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        [NotNull]
        IList<IObserver> Sources { get; }

        [CanBeNull]
        object GetValue([NotNull] IBindingMemberInfo targetMember, [NotNull] IDataContext context, bool throwOnError);

        bool SetValue([NotNull] IBindingSourceAccessor targetAccessor, [NotNull] IDataContext context, bool throwOnError);

        event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;
    }
}
