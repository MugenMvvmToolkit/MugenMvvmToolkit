#region Copyright

// ****************************************************************************
// <copyright file="IHasDisplayName.cs">
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
    ///     Represents the model that has a display name.
    /// </summary>
    public interface IHasDisplayName : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the display name for the current model.
        /// </summary>
        string DisplayName { get; set; }
    }
}