using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views
{
    public interface IInitializableView
    {
        void Initialize<TState>(IView view, in TState state, IReadOnlyMetadataContext? metadata);
    }
}