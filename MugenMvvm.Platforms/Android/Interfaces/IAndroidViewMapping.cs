using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Android.Interfaces
{
    public interface IAndroidViewMapping : IViewMapping
    {
        int ResourceId { get; }
    }
}