#region Copyright

// ****************************************************************************
// <copyright file="IDataBinding.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IDataBinding : IDisposable
    {
        bool IsDisposed { get; }

        [NotNull]
        IDataContext Context { get; }

        [NotNull]
        ISingleBindingSourceAccessor TargetAccessor { get; }

        [NotNull]
        IBindingSourceAccessor SourceAccessor { get; }

        [NotNull]
        ICollection<IBindingBehavior> Behaviors { get; }

        bool UpdateSource();

        bool UpdateTarget();

        bool Validate();

        event EventHandler<IDataBinding, BindingEventArgs> BindingUpdated;
    }
}
