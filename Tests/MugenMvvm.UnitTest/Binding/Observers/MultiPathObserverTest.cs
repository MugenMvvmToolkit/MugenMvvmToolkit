using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers.PathObservers;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class MultiPathObserverTest : MultiPathObserverTestBase<MultiPathObserver>
    {
        #region Methods

        protected override MultiPathObserver GetObserver(object target)
        {
            return new MultiPathObserver(target, DefaultPath, MemberFlags.All, false, false);
        }

        protected override MultiPathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
        {
            return new MultiPathObserver(target, path, memberFlags, hasStablePath, optional);
        }

        #endregion
    }
}