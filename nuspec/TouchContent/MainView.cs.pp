using CoreGraphics;
using Foundation;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Views;
using ObjCRuntime;
using UIKit;

namespace $rootnamespace$.Views
{
    [Register("MainView")]
    public class MainView : MvvmViewController
    {
        #region Overrides of MvvmViewController

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;

            if (RespondsToSelector(new Selector("edgesForExtendedLayout")))
                EdgesForExtendedLayout = UIRectEdge.None;

            var label = new UILabel(new CGRect(10, 10, View.Bounds.Width - 10, 40))
            {
                TextAlignment = UITextAlignment.Center
            };
            Add(label);

            using (var set = new BindingSet<MainViewModel>())
            {
                set.Bind(label, uiLabel => uiLabel.Text).To(vm => vm.Text);
            }
        }

        #endregion
    }
}