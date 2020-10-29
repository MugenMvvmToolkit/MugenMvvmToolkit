using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation.Observers;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
{
    public class MultiPathObserverTest : MultiPathObserverTestBase<MultiPathObserver>
    {
        #region Methods

        protected override MultiPathObserver GetObserver(object target) => new MultiPathObserver(target, DefaultPath, MemberFlags.All, false, false);

        protected override MultiPathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional) =>
            new MultiPathObserver(target, path, memberFlags, hasStablePath, optional);

        #endregion
    }
}