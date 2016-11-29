using Android.Views;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
{
    public interface IBindableMenuInflater
    {
        void Inflate(int menuRes, IMenu menu, object parent);
    }
}