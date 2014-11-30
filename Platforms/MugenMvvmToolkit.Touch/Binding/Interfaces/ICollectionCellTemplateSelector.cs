#region Copyright
// ****************************************************************************
// <copyright file="ICollectionCellTemplateSelector.cs">
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
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     CollectionCellTemplateSelector allows the app writer to provide custom template selection logic.
    /// </summary>
    public interface ICollectionCellTemplateSelector
    {
        /// <summary>
        ///     Initializes the current template selector.
        /// </summary>
        /// <param name="container"></param>
        void Initialize([NotNull] UICollectionView container);

        /// <summary>
        ///     Returns an app specific identifier for cell.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        NSString GetIdentifier([CanBeNull] object item, [NotNull] UICollectionView container);

        /// <summary>
        ///     Initializes an app specific template.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="cell">The specified cell to initialize.</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        void InitializeTemplate([NotNull] UICollectionView container, UICollectionViewCell cell);
    }
}