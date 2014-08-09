using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public abstract class BindingBehaviorTestBase<T> : BindingTestBase
        where T : IBindingBehavior
    {
        #region Properties

        protected BindingSourceAccessorMock TargetAccessorMock { get; private set; }

        protected BindingSourceAccessorMock SourceAccessorMock { get; private set; }

        protected BindingSourceMock TargetSourceMock { get; private set; }

        protected BindingSourceMock SourceSourceMock { get; private set; }

        protected DataBindingMock BindingMock { get; private set; }

        protected T Behavior { get; private set; }

        #endregion

        #region Methods

        [TestMethod]
        public virtual void BehaviorCanBeAttachedOnlyOnce()
        {
            BindingMock.UpdateSource = () => { };
            BindingMock.UpdateTarget = () => { };
            Behavior.Attach(BindingMock).ShouldBeTrue();
            ShouldThrow(() => Behavior.Attach(BindingMock));
        }

        [TestMethod]
        public virtual void BehaviorCanBeAttachedRepeatedly()
        {
            BindingMock.UpdateSource = () => { };
            BindingMock.UpdateTarget = () => { };
            Behavior.Attach(BindingMock).ShouldBeTrue();
            Behavior.Attach(BindingMock).ShouldBeTrue();
        }

        [TestMethod]
        public virtual void CloneTest()
        {
            Behavior.Clone().ShouldBeType(Behavior.GetType());
        }

        protected abstract T CreateBehavior();

        #endregion

        #region Overrides of BindingTestBase

        protected override void OnInit()
        {
            base.OnInit();
            Behavior = CreateBehavior();
            BindingMock = new DataBindingMock();
            TargetAccessorMock = new BindingSourceAccessorMock();
            SourceAccessorMock = new BindingSourceAccessorMock();
            TargetSourceMock = new BindingSourceMock();
            SourceSourceMock = new BindingSourceMock();
            SourceAccessorMock.Source = SourceSourceMock;
            TargetAccessorMock.Source = TargetSourceMock;
            BindingMock.TargetAccessor = TargetAccessorMock;
            BindingMock.SourceAccessor = SourceAccessorMock;
        }

        #endregion
    }
}