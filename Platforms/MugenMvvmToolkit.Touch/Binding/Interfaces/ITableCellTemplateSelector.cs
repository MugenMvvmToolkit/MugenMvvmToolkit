#region Copyright

// ****************************************************************************
// <copyright file="ITableCellTemplateSelector.cs">
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

using JetBrains.Annotations;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     TableCellTemplateSelector allows the app writer to provide custom template selection logic.
    /// </summary>
    public interface ITableCellTemplateSelector
    {
        /// <summary>
        ///     Returns an app specific identifier for cell.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        NSString GetIdentifier([CanBeNull] object item, [NotNull] UITableView container);

        /// <summary>
        ///     Returns an app specific template height.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="identifier">The specified identifier.</param>
        /// <returns>An app-specific template height to apply, or null.</returns>
        float? GetHeight([NotNull] UITableView container, [NotNull] NSString identifier);

        /// <summary>
        ///     Returns an app specific template.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="identifier">The specified identifier.</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        UITableViewCell SelectTemplate([NotNull] UITableView container, [NotNull] NSString identifier);
    }
}