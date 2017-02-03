using Android.App;
using Android.Views;

namespace MugenMvvmToolkit.Android.Interfaces.Views
{
    public interface IOptionsMenu
    {
        void Inflate(Activity activity, IMenu menu);
    }
}