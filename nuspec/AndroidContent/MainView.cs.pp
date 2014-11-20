using Android.App;
using MugenMvvmToolkit.Views.Activities;

namespace $rootnamespace$.Views
{
    [Activity(Label = "MainView")]
    public class MainView : MvvmActivity
    {
        #region Constructors

        public MainView()
            : base(Resource.Layout.Main)
        {
        }

        #endregion
    }
}