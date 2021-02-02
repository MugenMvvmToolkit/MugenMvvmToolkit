using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    public class MemberObserverTest : UnitTestBase
    {
        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(MemberObserver).IsEmpty.ShouldBeTrue();
            default(MemberObserver).TryObserve(this, new TestWeakEventListener(), DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryObserveShouldCallDelegate()
        {
            var count = 0;
            var target = new object();
            var listener = new TestWeakEventListener();
            var member = new object();
            var result = ActionToken.NoDoToken;
            var handler = new Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken>((t, m, l, meta) =>
            {
                ++count;
                t.ShouldEqual(target);
                m.ShouldEqual(member);
                l.ShouldEqual(listener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            });

            var observer = new MemberObserver(handler, member);
            observer.IsEmpty.ShouldBeFalse();
            observer.Deconstruct(out var h, out var m);
            handler.ShouldEqual(h);
            m.ShouldEqual(member);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
        }
    }
}