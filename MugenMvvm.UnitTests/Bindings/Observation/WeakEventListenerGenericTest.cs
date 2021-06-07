using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    [Collection(SharedContext)]
    public class WeakEventListenerGenericTest : UnitTestBase
    {
        public WeakEventListenerGenericTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var target = new TestWeakEventListener
            {
                IsWeak = true,
                IsAlive = true
            };
            var listener = new WeakEventListener<object>(target, this);
            listener.Target.ShouldEqual(target);
            listener.IsAlive.ShouldEqual(true);
            listener.Listener.ShouldEqual(target);
            listener.IsEmpty.ShouldBeFalse();
            listener.State.ShouldEqual(this);

            target.IsAlive = false;
            listener.IsAlive.ShouldEqual(false);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var target = new TestWeakEventListener
            {
                IsWeak = false,
                IsAlive = true
            };
            var listener = new WeakEventListener<object>(target, this);
            listener.State.ShouldEqual(this);
            ((IWeakReference)listener.Target!).Target.ShouldEqual(target);
            listener.IsAlive.ShouldEqual(true);
            listener.Listener.ShouldEqual(target);
            listener.IsEmpty.ShouldBeFalse();
        }

        [Fact]
        public void DefaultShouldBeEmpty() => default(WeakEventListener<object>).IsEmpty.ShouldBeTrue();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryHandleShouldUseListener(bool isWeak)
        {
            var sender = new object();
            var msg = new object();
            var result = true;
            var invokeCount = 0;
            var target = new TestWeakEventListener
            {
                IsWeak = isWeak,
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(sender);
                    o1.ShouldEqual(msg);
                    m.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var listener = new WeakEventListener<object>(target, this);
            listener.TryHandle(sender, msg, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            result = false;
            listener.TryHandle(sender, msg, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }
    }
}