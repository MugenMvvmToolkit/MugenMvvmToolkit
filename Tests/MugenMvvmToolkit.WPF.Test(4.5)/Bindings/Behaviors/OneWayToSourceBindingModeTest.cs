using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public class OneWayToSourceBindingModeTest : BindingModeTestBase<OneWayToSourceBindingMode>
    {
        #region Overrides of BindingModeTestBase<OneWayToSourceBindingMode>

        [Ignore]
        public override void BehaviorCanBeAttachedRepeatedly()
        {
        }

        [Ignore]
        public override void ModeShouldDoNothingOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldUpdateTargetOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldListenSourceChange()
        {
        }

        [Ignore]
        public override void ModeShouldNotListenAnySourceChange()
        {
        }

        protected override OneWayToSourceBindingMode CreateBehavior()
        {
            return new OneWayToSourceBindingMode();
        }

        #endregion
    }
}
