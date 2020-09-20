using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Android.Interfaces
{
    public interface IResourceViewMapping : IViewMapping
    {
        int ResourceId { get; }
    }
}