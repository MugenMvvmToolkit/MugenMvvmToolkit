using JetBrains.Annotations;
using MonoTouch.UIKit;

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