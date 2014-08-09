using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public class OneTimeBindingModeTest : BindingModeTestBase<OneTimeBindingMode>
    {
        #region Overrides of BindingModeTestBase<OneTimeBindingMode>

        [Ignore]
        public override void BehaviorCanBeAttachedRepeatedly()
        {
        }

        [Ignore]
        public override void ModeShouldDoNothingOnAttach()
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
        public override void ModeShouldNotListenAnySourceChange()
        {
        }

        protected override OneTimeBindingMode CreateBehavior()
        {
            return new OneTimeBindingMode();
        }

        #endregion
    }
}