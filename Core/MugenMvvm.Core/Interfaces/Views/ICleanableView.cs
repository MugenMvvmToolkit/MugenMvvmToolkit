using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views
{
    public interface ICleanableView
    {
        void Cleanup<TState>(in TState state, IReadOnlyMetadataContext? metadata);
    }
}