#region Copyright

// ****************************************************************************
// <copyright file="LinkerInclude.cs">
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
using UIKit;

namespace MugenMvvmToolkit
{
    internal static partial class LinkerInclude
    {
        [UsedImplicitly]
        private static void Include()
        {
            var barButton = new UIBarButtonItem();
            barButton.Clicked += (sender, args) => { };
            barButton.Clicked -= (sender, args) => { };
            barButton.Enabled = barButton.Enabled;

            var searchBar = new UISearchBar();
            searchBar.Text = searchBar.Text;
            searchBar.TextChanged += (sender, args) => { };
            searchBar.TextChanged -= (sender, args) => { };

            var control = new UIControl();
            control.ValueChanged += (sender, args) => { };
            control.ValueChanged -= (sender, args) => { };
            control.TouchUpInside += (sender, args) => { };
            control.TouchUpInside -= (sender, args) => { };
        }
    }
}