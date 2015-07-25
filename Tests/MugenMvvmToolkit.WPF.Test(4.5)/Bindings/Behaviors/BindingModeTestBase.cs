using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public abstract class BindingModeTestBase<T> : BindingBehaviorTestBase<T>
        where T : IBindingBehavior
    {
        #region Methods

        [TestMethod]
        public void BehaviorShouldUseModeId()
        {
            Behavior.Id.ShouldEqual(BindingModeBase.IdBindingMode);
        }

        [TestMethod]
        public virtual void ModeShouldDoNothingOnAttach()
        {
            bool isTargetInvoked = false;
            bool isSourceInvoked = false;
            BindingMock.UpdateTarget = () => isTargetInvoked = true;
            BindingMock.UpdateSource = () => isSourceInvoked = true;

            Behavior.Attach(BindingMock).ShouldBeTrue();
            isTargetInvoked.ShouldBeFalse();
            isSourceInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public virtual void ModeShouldUpdateSourceOnAttach()
        {
            bool isTargetInvoked = false;
            bool isSourceInvoked = false;
            BindingMock.UpdateTarget = () => isTargetInvoked = true;
            BindingMock.UpdateSource = () => isSourceInvoked = true;
            TargetSourceMock.GetPathMembers = b => new BindingPathMembersMock(SourceSourceMock, BindingPath.DataContext);

            Behavior.Attach(BindingMock).ShouldBeTrue();
            isTargetInvoked.ShouldBeFalse();
            isSourceInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void ModeShouldUpdateTargetOnAttach()
        {
            bool isTargetInvoked = false;
            bool isSourceInvoked = false;
            BindingMock.UpdateTarget = () => isTargetInvoked = true;
            BindingMock.UpdateSource = () => isSourceInvoked = true;
            SourceSourceMock.GetPathMembers = b => new BindingPathMembersMock(SourceSourceMock, BindingPath.DataContext);

            Behavior.Attach(BindingMock).ShouldBeTrue();

            isTargetInvoked.ShouldBeTrue();
            isSourceInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public virtual void ModeShouldListenTargetChange()
        {
            bool isTargetInvoked = false;
            bool isSourceInvoked = false;
            BindingMock.UpdateTarget = () => isTargetInvoked = true;
            BindingMock.UpdateSource = () => isSourceInvoked = true;

            Behavior.Attach(BindingMock).ShouldBeTrue();
            isSourceInvoked = false;
            isTargetInvoked = false;

            TargetSourceMock.RaiseValueChanged();
            isTargetInvoked.ShouldBeFalse();
            isSourceInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void ModeShouldListenSourceChange()
        {
            SourceSourceMock.GetPathMembers = b => new BindingPathMembersMock(new object(), BindingPath.Empty);
            bool isTargetInvoked = false;
            bool isSourceInvoked = false;
            BindingMock.UpdateTarget = () => isTargetInvoked = true;
            BindingMock.UpdateSource = () => isSourceInvoked = true;

            Behavior.Attach(BindingMock).ShouldBeTrue();
            isSourceInvoked = false;
            isTargetInvoked = false;

            SourceSourceMock.RaiseValueChanged();
            isTargetInvoked.ShouldBeTrue();
            isSourceInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public virtual void ModeShouldNotListenAnySourceChange()
        {
            bool isTargetInvoked = false;
            bool isSourceInvoked = false;
            BindingMock.UpdateTarget = () => isTargetInvoked = true;
            BindingMock.UpdateSource = () => isSourceInvoked = true;

            Behavior.Attach(BindingMock).ShouldBeTrue();
            isSourceInvoked = false;
            isTargetInvoked = false;

            SourceSourceMock.RaiseValueChanged();
            TargetSourceMock.RaiseValueChanged();
            isTargetInvoked.ShouldBeFalse();
            isSourceInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public virtual void ModeShouldDoNothingOnDetach()
        {
            bool isTargetInvoked = false;
            bool isSourceInvoked = false;
            BindingMock.UpdateTarget = () => isTargetInvoked = true;
            BindingMock.UpdateSource = () => isSourceInvoked = true;

            Behavior.Attach(BindingMock).ShouldBeTrue();
            isSourceInvoked = false;
            isTargetInvoked = false;
            Behavior.Detach(BindingMock);

            TargetSourceMock.RaiseValueChanged();
            SourceSourceMock.RaiseValueChanged();
            isTargetInvoked.ShouldBeFalse();
            isSourceInvoked.ShouldBeFalse();
        }

        #endregion
    }
}