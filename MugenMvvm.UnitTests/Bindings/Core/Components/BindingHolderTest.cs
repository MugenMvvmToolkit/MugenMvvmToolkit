using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingHolderTest : UnitTestBase
    {
        private readonly BindingHolder _bindingHolder;

        public BindingHolderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _bindingHolder = new BindingHolder(AttachedValueManager);
            BindingManager.AddComponent(_bindingHolder);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void BindingHolderShouldKeepBindingsUsingTargetPath(int count)
        {
            const string defaultPath = "Test.Test";

            var bindings = new List<IBinding>();
            for (var i = 0; i < count; i++)
            {
                var testBinding = new TestBinding(ComponentCollectionManager) { Target = new TestMemberPathObserver { Path = MemberPath.Get(defaultPath + i) } };
                bindings.Add(testBinding);

                _bindingHolder.TryRegister(BindingManager, this, testBinding, Metadata).ShouldBeTrue();
                BindingManager.GetBindings(this, defaultPath + i, Metadata).Single().ShouldEqual(testBinding);
                var array = BindingManager.GetBindings(this, null, Metadata);
                array.Count.ShouldEqual(bindings.Count);
                array.ShouldContain(bindings);
            }

            for (var i = 0; i < count; i++)
            {
                _bindingHolder.TryUnregister(BindingManager, this, bindings[i], Metadata).ShouldBeTrue();
                _bindingHolder.TryGetBindings(BindingManager, this, defaultPath + i, Metadata).ShouldBeEmpty();
                var array = BindingManager.GetBindings(this, null, Metadata);
                array.Count.ShouldEqual(bindings.Count - i - 1);
                array.ShouldContain(bindings.Skip(i + 1));
            }
        }

        [Fact]
        public void TryRegisterShouldDisposePrevBinding()
        {
            var b1Disposed = false;
            var b2Disposed = false;
            var b1 = new TestBinding(ComponentCollectionManager)
            {
                Target = new TestMemberPathObserver { Path = MemberPath.Get("T") },
                Dispose = () => b1Disposed = true
            };
            var b2 = new TestBinding(ComponentCollectionManager)
            {
                Target = new TestMemberPathObserver { Path = MemberPath.Get("T") },
                Dispose = () => b2Disposed = true
            };

            _bindingHolder.TryRegister(BindingManager, this, b1, Metadata).ShouldBeTrue();
            b1Disposed.ShouldBeFalse();
            b2Disposed.ShouldBeFalse();

            _bindingHolder.TryRegister(BindingManager, this, b2, Metadata).ShouldBeTrue();
            b1Disposed.ShouldBeTrue();
            b2Disposed.ShouldBeFalse();
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);
    }
}