using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
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
        }

        [Fact]
        public void TryRegisterShouldDisposePrevBinding()
        {
            var b1Disposed = false;
            var b2Disposed = false;
            var b1 = new TestBinding(ComponentCollectionManager)
            {
                Target = new TestMemberPathObserver {Path = MemberPath.Get("T")},
                Dispose = () => b1Disposed = true
            };
            var b2 = new TestBinding(ComponentCollectionManager)
            {
                Target = new TestMemberPathObserver {Path = MemberPath.Get("T")},
                Dispose = () => b2Disposed = true
            };

            _bindingHolder.TryRegister(null!, this, b1, DefaultMetadata).ShouldBeTrue();
            b1Disposed.ShouldBeFalse();
            b2Disposed.ShouldBeFalse();

            _bindingHolder.TryRegister(null!, this, b2, DefaultMetadata).ShouldBeTrue();
            b1Disposed.ShouldBeTrue();
            b2Disposed.ShouldBeFalse();
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
                var testBinding = new TestBinding(ComponentCollectionManager) {Target = new TestMemberPathObserver {Path = MemberPath.Get(defaultPath + i)}};
                bindings.Add(testBinding);

                _bindingHolder.TryRegister(null!, this, testBinding, DefaultMetadata).ShouldBeTrue();
                _bindingHolder.TryGetBindings(null!, this, defaultPath + i, DefaultMetadata).AsList().Single().ShouldEqual(testBinding);
                var array = _bindingHolder.TryGetBindings(null!, this, null, DefaultMetadata).AsList();
                array.Count.ShouldEqual(bindings.Count);
                array.ShouldContain(bindings);
            }

            for (var i = 0; i < count; i++)
            {
                _bindingHolder.TryUnregister(null!, this, bindings[i], DefaultMetadata).ShouldBeTrue();
                _bindingHolder.TryGetBindings(null!, this, defaultPath + i, DefaultMetadata).AsList().ShouldBeEmpty();
                var array = _bindingHolder.TryGetBindings(null!, this, null, DefaultMetadata).AsList();
                array.Count.ShouldEqual(bindings.Count - i - 1);
                array.ShouldContain(bindings.Skip(i + 1));
            }
        }
    }
}