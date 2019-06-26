using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views
{
    public interface ICleanableView : IView
    {
        void Cleanup(IReadOnlyMetadataContext? metadata);
    }
}