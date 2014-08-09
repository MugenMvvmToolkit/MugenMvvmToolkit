using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public class TwoWayBindingModeTest : BindingModeTestBase<TwoWayBindingMode>
    {
        #region Overrides of BindingModeTestBase<TwoWayBindingMode>

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
        public override void ModeShouldNotListenAnySourceChange()
        {            
        }

        protected override TwoWayBindingMode CreateBehavior()
        {
            return new TwoWayBindingMode();
        }

        #endregion
    }
}