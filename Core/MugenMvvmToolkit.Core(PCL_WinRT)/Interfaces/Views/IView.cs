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

using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represent the interface-marker for view.
    /// </summary>
    public interface IView
    {
    }

    /// <summary>
    ///     Represents the interface that allows to get access to the view model.
    /// </summary>
    public interface IViewModelAwareView<TViewModel> : IView where TViewModel : IViewModel
    {
        /// <summary>
        ///     Gets or sets the view-model.
        /// </summary>
        TViewModel ViewModel { get; set; }
    }
}