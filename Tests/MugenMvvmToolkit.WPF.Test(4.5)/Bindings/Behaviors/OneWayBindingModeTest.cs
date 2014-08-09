using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public class OneWayBindingModeTest : BindingModeTestBase<OneWayBindingMode>
    {
        #region Overrides of BindingModeTestBase<OneWayBindingMode>

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
        public override void ModeShouldListenTargetChange()
        {
        }

        [Ignore]
        public override void ModeShouldNotListenAnySourceChange()
        {
        }

        protected override OneWayBindingMode CreateBehavior()
        {
            return new OneWayBindingMode();
        }

        #endregion
    }
}