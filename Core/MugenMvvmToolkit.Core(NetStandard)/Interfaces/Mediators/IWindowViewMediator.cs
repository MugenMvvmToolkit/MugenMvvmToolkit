#region Copyright

// ****************************************************************************
// <copyright file="IWindowViewMediator.cs">
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

using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Mediators
{
    public interface IWindowViewMediator
    {
        bool IsOpen { get; }

        [CanBeNull]
        object View { get; }

        [NotNull]
        IViewModel ViewModel { get; }

        void Initialize([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);

        Task ShowAsync([CanBeNull] IDataContext context);

        Task<bool> CloseAsync([CanBeNull] IDataContext context);

        void UpdateView([CanBeNull] object view, bool isOpen, [CanBeNull] IDataContext context);
    }
}
