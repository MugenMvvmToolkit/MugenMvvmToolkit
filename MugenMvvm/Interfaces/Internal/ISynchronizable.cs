using JetBrains.Annotations;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ISynchronizable
    {
        ILocker Locker { get; }

        void UpdateLocker(ILocker locker);

        [MustUseReturnValue]
        ActionToken Lock();
    }
}