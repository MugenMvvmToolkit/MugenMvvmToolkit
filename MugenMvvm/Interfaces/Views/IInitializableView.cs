using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views
{
    public interface IInitializableView
    {
        void Initialize(IView view, object? state, IReadOnlyMetadataContext? metadata);
    }
}