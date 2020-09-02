#if XAMARIN_IOS
// ReSharper disable once CheckNamespace
namespace MugenMvvm.Ios.Interfaces
#else
// ReSharper disable once CheckNamespace
namespace MugenMvvm.Android.Interfaces
#endif
{
    public interface IItemsSourceEqualityComparer
    {
        bool AreItemsTheSame(object? x, object? y);
    }
}