#region Copyright

// ****************************************************************************
// <copyright file="INavigationProvider.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface INavigationProvider : IDisposable
    {
        [CanBeNull]
        IViewModel CurrentViewModel { get; }

        [CanBeNull]
        object CurrentContent { get; }

        [NotNull]
        Task CurrentNavigationTask { get; }

        Task<bool> NavigateAsync([NotNull] IDataContext context);

        [CanBeNull]
        Task<bool> TryCloseAsync([NotNull] IDataContext context);

        void Restore([NotNull] IDataContext context);
    }
}
