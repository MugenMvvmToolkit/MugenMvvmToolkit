using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Requests
{
    public interface IStateRequest : IMetadataOwner<IMetadataContext>, ICancelableRequest
    {
    }
}