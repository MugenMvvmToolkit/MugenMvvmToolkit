using Android.Content;
using Android.Views;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
{
    public interface IMenuTemplate
    {
        void Apply(IMenu menu, Context context, object parent);        
    }
}