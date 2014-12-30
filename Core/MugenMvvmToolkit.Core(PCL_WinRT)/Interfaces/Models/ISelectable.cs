#region Copyright

// ****************************************************************************
// <copyright file="ISelectable.cs">
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

using System.ComponentModel;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the model that can be selected.
    /// </summary>
    public interface ISelectable : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the property that indicates that current model is selected.
        /// </summary>
        bool IsSelected { get; set; }
    }
}