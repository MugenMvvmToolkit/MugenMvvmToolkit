#region Copyright
// ****************************************************************************
// <copyright file="IView.cs">
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
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represent the base interface for view.
    /// </summary>
    public interface IView
    {
        /// <summary>
        ///     Gets or sets the data context of the current <see cref="IView" />.
        /// </summary>
        [CanBeNull]
        object DataContext { get; set; }
    }

    /// <summary>
    ///     Adds support for the view-model in view.
    /// </summary>
    public interface IViewModelAwareView<TViewModel> where TViewModel : IViewModel
    {
        /// <summary>
        ///     Gets or sets the view-model.
        /// </summary>
        TViewModel ViewModel { get; set; }
    }
}