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

namespace MugenMvvmToolkit.WinPhone.AppBar
{
    public class BindableApplicationBarIconButton : BindableApplicationBarMenuItem, IApplicationBarIconButton
    {
        #region Fields

        public static readonly DependencyProperty IconUriProperty =
            DependencyProperty.Register("IconUri", typeof (Uri), typeof (BindableApplicationBarIconButton),
                new PropertyMetadata((o, args) =>
                    ((BindableApplicationBarIconButton) o).ApplicationBarItem.IconUri = (Uri) args.NewValue));

        #endregion

        #region Properties

        public new IApplicationBarIconButton ApplicationBarItem
        {
            get { return (IApplicationBarIconButton) base.ApplicationBarItem; }
        }

        public Uri IconUri
        {
            get { return (Uri) GetValue(IconUriProperty); }
            set { SetValue(IconUriProperty, value); }
        }

        #endregion

        #region Overrides of BindableApplicationBarMenuItem

        protected override IList OriginalList
        {
            get
            {
                if (ApplicationBar == null || ApplicationBar.OriginalApplicationBar == null)
                    return null;
                return ApplicationBar.OriginalApplicationBar.Buttons;
            }
        }

        protected override IApplicationBarMenuItem CreateApplicationBarItem()
        {
            return new ApplicationBarIconButton();
        }

        #endregion
    }
}
