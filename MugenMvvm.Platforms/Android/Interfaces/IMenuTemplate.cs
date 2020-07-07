using Android.Views;

namespace MugenMvvm.Android.Interfaces
{
    public interface IMenuTemplate
    {
        void Apply(IMenu menu, object owner);

        void Clear(IMenu menu);
    }
}