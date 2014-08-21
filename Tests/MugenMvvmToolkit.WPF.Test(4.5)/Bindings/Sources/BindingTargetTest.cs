using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Sources;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Sources
{
    [TestClass]
    public class BindingTargetTest : BindingSourceTest
    {
        #region Methods

        [TestMethod]
        public void SetErrorsShouldCallErrorsMember()
        {
            var target = new object();
            var list = new List<object> { "Test" };
            bool isInvoked = false;

            BindingServiceProvider.ErrorProvider = new BindingErrorProviderMock
            {
                SetErrors = (o, objects) =>
                {
                    o.ShouldEqual(target);
                    objects.SequenceEqual(list).ShouldBeTrue();
                    isInvoked = true;
                }
            };
            var mock = new ObserverMock { PathMembers = new BindingPathMembersMock(target, BindingPath.Empty) };
            var bindingTarget = (BindingTarget)CreateBindingSource(mock);
            bindingTarget.SetErrors(new SenderType("Test"), list);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void BindingTargetShouldUseDelegatesForIsEnabledProperty()
        {
            bool isEnabled = false;
            IAttachedBindingMemberInfo<object, bool> member =
                AttachedBindingMember.CreateMember<object, bool>(AttachedMemberConstants.Enabled, (info, o, arg3) => isEnabled,
                    (info, o, arg3) => isEnabled = (bool)arg3[0]);
            var memberProvider = new BindingMemberProvider();
            memberProvider.Register(typeof(object), member, false);
            BindingServiceProvider.MemberProvider = memberProvider;

            var mock = new ObserverMock { PathMembers = new BindingPathMembersMock(new object(), BindingPath.Empty) };
            var bindingTarget = (BindingTarget)CreateBindingSource(mock);

            bindingTarget.IsEnabled.ShouldBeFalse();
            bindingTarget.IsEnabled = true;
            isEnabled.ShouldBeTrue();
            bindingTarget.IsEnabled.ShouldBeTrue();

            bindingTarget.IsEnabled = false;
            isEnabled.ShouldBeFalse();
            bindingTarget.IsEnabled.ShouldBeFalse();
        }

        #endregion

        #region Overrides of BindingSourceTest

        protected override BindingSource CreateBindingSource(IObserver observer)
        {
            return new BindingTarget(observer);
        }

        #endregion
    }
}