using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Members.Internal
{
    public class TestEventInfo : TestMemberInfoBase, IEventInfo
    {
        #region Implementation of interfaces

        ActionToken IEventInfo.TrySubscribe(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return TryObserve?.Invoke(target, listener, metadata) ?? default;
        }

        #endregion
    }
}