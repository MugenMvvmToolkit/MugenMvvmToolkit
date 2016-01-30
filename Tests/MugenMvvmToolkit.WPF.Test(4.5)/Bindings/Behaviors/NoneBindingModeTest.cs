using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public class NoneBindingModeTest : BindingModeTestBase<NoneBindingMode>
    {
        #region Overrides of BindingModeTestBase<NoneBindingMode>

        protected override NoneBindingMode CreateBehavior()
        {
            return NoneBindingMode.Instance;
        }

        [Ignore]
        public override void BehaviorCanBeAttachedOnlyOnce()
        {
        }

        [Ignore]
        public override void ModeShouldUpdateSourceOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldUpdateTargetOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldListenTargetChange()
        {
        }

        [Ignore]
        public override void ModeShouldListenSourceChange()
        {
        }

        #endregion
    }
}
