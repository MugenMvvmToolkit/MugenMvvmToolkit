#region Copyright

// ****************************************************************************
// <copyright file="IWindowViewMediator.cs">
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

using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
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

        Task ShowAsync([CanBeNull] IOperationCallback callback, [CanBeNull] IDataContext context);

        Task<bool> CloseAsync([CanBeNull] object parameter);

        void UpdateView([CanBeNull] object view, bool isOpen, [CanBeNull] IDataContext context);
    }
}
