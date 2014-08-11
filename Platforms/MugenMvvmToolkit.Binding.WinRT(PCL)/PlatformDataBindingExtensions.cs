#region Copyright
// ****************************************************************************
// <copyright file="PlatformDataBindingExtensions.cs">
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding
{
    /// <summary>
    ///     Represents the platform specific extensions.
    /// </summary>
    public static class PlatformDataBindingExtensions
    {
        #region Methods

        /// <summary>
        ///     Shows the flyout placed in relation to the specified element, and use this element as parent for content items.
        /// </summary>
        /// <param name="flyoutBase">The specified flayout.</param>
        /// <param name="placementTarget">The element to use as the flyout's placement target.</param>
        public static void ShowAtEx([NotNull] this FlyoutBase flyoutBase, [NotNull] FrameworkElement placementTarget)
        {
            Should.NotBeNull(flyoutBase, "flyoutBase");
            Should.NotBeNull(placementTarget, "placementTarget");
            var flyout = flyoutBase as Flyout;
            if (flyout != null)
                PlatformDataBindingModule.SetAttachedParent(flyout.Content as FrameworkElement, placementTarget);
            var menuFlyout = flyoutBase as MenuFlyout;
            if (menuFlyout != null && menuFlyout.Items != null)
            {
                foreach (MenuFlyoutItemBase item in menuFlyout.Items)
                    PlatformDataBindingModule.SetAttachedParent(item, placementTarget);
            }
            flyoutBase.ShowAt(placementTarget);
        }

        #endregion

    }
}