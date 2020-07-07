using Android.Views;

namespace MugenMvvm.Android.Interfaces
{
    public interface IMenuItemTemplate
    {
        void Apply(IMenu menu, int id, int order, object? item);

        void Clear(IMenuItem menuItem);
    }
}