#region Copyright
// ****************************************************************************
// <copyright file="IWrapperViewModel.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the base inteface for view model wrapper.
    /// </summary>
    public interface IWrapperViewModel : IViewModel
    {
        /// <summary>
        ///     Gets the underlying view model.
        /// </summary>
        IViewModel ViewModel { get; }

        /// <summary>
        ///     Wraps the specified view-model.
        /// </summary>
        void Wrap([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);
    }
}