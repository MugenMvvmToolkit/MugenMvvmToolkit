#region Copyright

// ****************************************************************************
// <copyright file="IViewAwareViewModel.cs">
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

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the interface that allows to get access to the view.
    /// </summary>
    public interface IViewAwareViewModel<TView> : IViewModel where TView : class
    {
        /// <summary>
        ///     Gets or sets the view object.
        /// </summary>
        TView View { get; set; }
    }
}