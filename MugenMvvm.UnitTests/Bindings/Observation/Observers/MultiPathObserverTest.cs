using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Enums;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
{
    public class MultiPathObserverTest : MultiPathObserverTestBase<MultiPathObserver>
    {
        protected override MultiPathObserver GetObserver(object target) => new(target, DefaultPath, MemberFlags.All, false, false, false);

        protected override MultiPathObserver GetObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool hasStablePath, bool optional) =>
            new(target, path, memberFlags, hasStablePath, optional, true);
    }
}