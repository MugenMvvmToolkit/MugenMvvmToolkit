using JetBrains.Annotations;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MugenMvvmToolkit.Interfaces
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