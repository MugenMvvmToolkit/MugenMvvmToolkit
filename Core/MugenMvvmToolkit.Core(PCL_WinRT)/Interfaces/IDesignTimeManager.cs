#region Copyright

// ****************************************************************************
// <copyright file="IDesignTimeManager.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IDesignTimeManager : IDisposable
    {
        bool IsDesignMode { get; }

        int Priority { get; }

        PlatformInfo Platform { get; }

        [CanBeNull]
        IIocContainer IocContainer { get; }

        [CanBeNull]
        IDataContext Context { get; }

        void Initialize();

        void InitializeViewModel([NotNull] IViewModel viewModel);
    }
}
