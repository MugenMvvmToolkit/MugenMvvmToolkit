using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    public class WeakEventListenerTest : UnitTestBase
    {
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
            var listener = new WeakEventListener(target);
            listener.TryHandle(sender, msg, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            result = false;
            listener.TryHandle(sender, msg, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var target = new TestWeakEventListener
            {
                IsWeak = true,
                IsAlive = true
            };
            var listener = new WeakEventListener(target);
            listener.Target.ShouldEqual(target);
            listener.IsAlive.ShouldEqual(true);
            listener.Listener.ShouldEqual(target);
            listener.IsEmpty.ShouldBeFalse();

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
            var listener = new WeakEventListener(target);
            ((IWeakReference) listener.Target!).Target.ShouldEqual(target);
            listener.IsAlive.ShouldEqual(true);
            listener.Listener.ShouldEqual(target);
            listener.IsEmpty.ShouldBeFalse();
        }

        [Fact]
        public void DefaultShouldBeEmpty() => default(WeakEventListener).IsEmpty.ShouldBeTrue();
    }
}