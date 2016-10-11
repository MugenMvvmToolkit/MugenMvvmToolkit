#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingExtensions.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.UWP.Binding.Models;

namespace MugenMvvmToolkit.UWP.Binding
{
    public static partial class PlatformDataBindingExtensions
    {
        #region Methods

        public static void ShowAtEx([NotNull] this FlyoutBase flyoutBase, [NotNull] FrameworkElement placementTarget)
        {
            Should.NotBeNull(flyoutBase, nameof(flyoutBase));
            Should.NotBeNull(placementTarget, nameof(placementTarget));
            var flyout = flyoutBase as Flyout;
            if (flyout == null)
            {
                var menuFlyout = flyoutBase as MenuFlyout;
                if (menuFlyout != null && menuFlyout.Items != null)
                {
                    foreach (MenuFlyoutItemBase item in menuFlyout.Items)
                        ParentObserver.GetOrAdd(item).Parent = placementTarget;
                }
            }
            else
            {
                var content = flyout.Content as FrameworkElement;
                if (content != null)
                    ParentObserver.GetOrAdd(content).Parent = placementTarget;
            }
            flyoutBase.ShowAt(placementTarget);
        }

        #endregion
    }
}
