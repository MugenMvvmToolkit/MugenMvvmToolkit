#region Copyright

// ****************************************************************************
// <copyright file="BindableApplicationBarIconButton.cs">
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

using System;
using System.Collections;
using System.Windows;
using Microsoft.Phone.Shell;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Controls
{
    /// <summary>
    ///     An bindable Application Bar button with an icon.
    /// </summary>
    public class BindableApplicationBarIconButton : BindableApplicationBarMenuItem, IApplicationBarIconButton
    {
        #region Fields

        /// <summary>
        ///     Identifies the <see cref="IconUri" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconUriProperty =
            DependencyProperty.Register("IconUri", typeof (Uri), typeof (BindableApplicationBarIconButton),
                new PropertyMetadata((o, args) =>
                    ((BindableApplicationBarIconButton) o).ApplicationBarItem.IconUri = (Uri) args.NewValue));

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the original application bar item.
        /// </summary>
        public new IApplicationBarIconButton ApplicationBarItem
        {
            get { return (IApplicationBarIconButton) base.ApplicationBarItem; }
        }

        /// <summary>
        ///     The URI of the icon to use for the button.
        /// </summary>
        /// <returns>
        ///     Type: <see cref="T:System.Uri" />.
        /// </returns>
        public Uri IconUri
        {
            get { return (Uri) GetValue(IconUriProperty); }
            set { SetValue(IconUriProperty, value); }
        }

        #endregion

        #region Overrides of BindableApplicationBarMenuItem

        /// <summary>
        ///     Gets the original list of items.
        /// </summary>
        protected override IList OriginalList
        {
            get
            {
                if (ApplicationBar == null || ApplicationBar.OriginalApplicationBar == null)
                    return null;
                return ApplicationBar.OriginalApplicationBar.Buttons;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="IApplicationBarMenuItem" />
        /// </summary>
        protected override IApplicationBarMenuItem CreateApplicationBarItem()
        {
            return new ApplicationBarIconButton();
        }

        #endregion
    }
}