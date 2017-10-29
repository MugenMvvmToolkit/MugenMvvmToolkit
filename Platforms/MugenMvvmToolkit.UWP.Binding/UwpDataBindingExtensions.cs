#region Copyright

// ****************************************************************************
// <copyright file="UwpDataBindingExtensions.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.UWP.Binding.Models;
using MugenMvvmToolkit.UWP.Infrastructure;

namespace MugenMvvmToolkit.UWP.Binding
{
    public static class UwpDataBindingExtensions
    {
        #region Fields

        private static bool _initializedFromDesign;

        #endregion

        #region Methods

        public static void ShowAtEx([NotNull] this FlyoutBase flyoutBase, [NotNull] FrameworkElement placementTarget)
        {
            Should.NotBeNull(flyoutBase, nameof(flyoutBase));
            Should.NotBeNull(placementTarget, nameof(placementTarget));
            var flyout = flyoutBase as Flyout;
            if (flyout == null)
            {
                var items = (flyoutBase as MenuFlyout)?.Items;
                if (items != null)
                {
                    foreach (MenuFlyoutItemBase item in items)
                        ParentObserver.Set(item, placementTarget);
                }
            }
            else
            {
                var content = flyout.Content as FrameworkElement;
                if (content != null)
                    ParentObserver.Set(content, placementTarget);
            }
            flyoutBase.ShowAt(placementTarget);
        }

        public static void InitializeFromDesignContext()
        {
            BindingServiceProvider.InitializeFromDesignContext();
            if (!_initializedFromDesign)
            {
                _initializedFromDesign = true;
                var methodInfo = typeof(UwpDataBindingExtensions).GetMethodEx(nameof(InitializeFromDesignContextInternal), MemberFlags.Static | MemberFlags.NonPublic | MemberFlags.Public);
                methodInfo?.Invoke(null, null);
            }
        }

        internal static void InitializeFromDesignContextInternal()
        {
            BindingServiceProvider.ValueConverter = BindingConverterExtensions.Convert;
            if (ToolkitServiceProvider.AttachedValueProvider == null)
                ToolkitServiceProvider.AttachedValueProvider = new AttachedValueProvider();
            if (ToolkitServiceProvider.ReflectionManager == null)
                ToolkitServiceProvider.ReflectionManager = new ExpressionReflectionManager();
            if (ToolkitServiceProvider.ThreadManager == null)
                ToolkitServiceProvider.ThreadManager = new SynchronousThreadManager();
        }

        #endregion
    }
}
