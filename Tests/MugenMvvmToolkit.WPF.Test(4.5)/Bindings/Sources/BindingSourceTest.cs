using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Sources;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Sources
{
    [TestClass]
    public class BindingSourceTest : BindingTestBase
    {
        #region Fields

        protected static readonly PropertyInfo PropertyInfo = typeof(BindingTestBase)
            .GetPropertyEx("EmptyContext", MemberFlags.Instance | MemberFlags.NonPublic | MemberFlags.Public);

        #endregion

        #region Methods

        [TestMethod]
        public void SourceShouldReturnPathFromObserver()
        {
            var mock = new ObserverMock();
            BindingSource target = CreateBindingSource(mock);
            target.Path.ShouldBeNull();
            mock.Path = BindingPath.Create("test");
            target.Path.ShouldEqual(mock.Path);
        }

        [TestMethod]
        public void SourceShouldReturnMembersFromObserver()
        {
            var mock = new ObserverMock { PathMembers = UnsetBindingPathMembers.Instance };
            BindingSource target = CreateBindingSource(mock);
            IBindingPathMembers pathMembers = target.GetPathMembers(true);
            pathMembers.ShouldEqual(UnsetBindingPathMembers.Instance);
            pathMembers.AllMembersAvailable.ShouldBeFalse();

            mock.PathMembers = new BindingPathMembersMock(this, BindingPath.Empty,
                new BindingMemberInfo(PropertyInfo.Name, PropertyInfo, PropertyInfo.DeclaringType));
            pathMembers = target.GetPathMembers(true);
            pathMembers.ShouldEqual(mock.PathMembers);
            pathMembers.AllMembersAvailable.ShouldBeTrue();
        }

        [TestMethod]
        public void SourceShouldReturnSourceFromObserver()
        {
            var src = new object();
            var mock = new ObserverMock
            {
                GetActualSource = b => src
            };
            BindingSource target = CreateBindingSource(mock);
            target.GetSource(true).ShouldEqual(src);
        }

        [TestMethod]
        public void SourceShouldThrowExceptionWhenReturnSourceIfObserverHasException()
        {
            var mock = new ObserverMock
            {
                GetActualSource = b => { throw new TestException(); }
            };
            BindingSource target = CreateBindingSource(mock);
            ShouldThrow<TestException>(() => target.GetSource(true));
        }

        [TestMethod]
        public void SourceShouldReturnValidateFromObserver()
        {
            bool param = false;
            bool result = false;
            var mock = new ObserverMock
            {
                Validate = b =>
                {
                    b.ShouldEqual(param);
                    return result;
                }
            };
            BindingSource target = CreateBindingSource(mock);

            target.Validate(param).ShouldBeFalse();

            param = true;
            result = true;
            target.Validate(param).ShouldBeTrue();
        }

        [TestMethod]
        public void SourceShouldThrowExceptionIfObserverHasException()
        {
            var mock = new ObserverMock
            {
                Validate = b => { throw new TestException(); }
            };
            var target = CreateBindingSource(mock);
            ShouldThrow<TestException>(() => target.Validate(true));
        }

        [TestMethod]
        public void SourceShouldRaiseValueChangedWhenObserverRaisesValueChanged()
        {
            bool isInvoked = false;
            var mock = new ObserverMock();
            BindingSource target = CreateBindingSource(mock);

            target.ValueChanged += (sender, args) => isInvoked = true;
            isInvoked.ShouldBeFalse();
            mock.RaiseValueChanged();
            isInvoked.ShouldBeTrue();
        }

        protected virtual BindingSource CreateBindingSource(IObserver observer)
        {
            return new BindingSource(observer);
        }

        #endregion
    }
}