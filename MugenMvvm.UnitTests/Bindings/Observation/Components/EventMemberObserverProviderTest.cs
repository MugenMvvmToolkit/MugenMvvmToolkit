using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class EventMemberObserverProviderTest : UnitTestBase
    {
        private readonly EventMemberObserverProvider _provider;

        public EventMemberObserverProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _provider = new EventMemberObserverProvider(MemberManager);
            ObservationManager.AddComponent(_provider);
        }

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest() =>
            ObservationManager.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();

        [Fact]
        public void TryGetMemberObserverShouldUseEventFinder1()
        {
            var target = new object();
            var listener = new TestWeakEventListener();
            var token = ActionToken.NoDo;
            var tryObserveCount = 0;

            var invokeCount = 0;
            var targetType = typeof(string);
            var member = typeof(EventMemberObserverProviderTest).GetMethod(nameof(TryGetMemberObserverShouldUseEventFinder1))!;
            var result = new TestEventInfo
            {
                TryObserve = (o, l, arg3) =>
                {
                    ++tryObserveCount;
                    o.ShouldEqual(target);
                    l.ShouldEqual(listener);
                    arg3.ShouldEqual(DefaultMetadata);
                    return token;
                }
            };

            _provider.EventFinder = (type, o, arg3) =>
            {
                ++invokeCount;
                type.ShouldEqual(targetType);
                member.ShouldEqual(o);
                arg3.ShouldEqual(DefaultMetadata);
                return result;
            };

            var observer = ObservationManager.TryGetMemberObserver(targetType, member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldUseEventFinder2()
        {
            var target = new object();
            var listener = new TestWeakEventListener();
            var token = ActionToken.NoDo;
            var tryObserveCount = 0;

            var invokeCount = 0;
            var targetType = typeof(string);
            var member = this;
            var result = new TestEventInfo
            {
                TryObserve = (o, l, arg3) =>
                {
                    ++tryObserveCount;
                    o.ShouldEqual(target);
                    l.ShouldEqual(listener);
                    arg3.ShouldEqual(DefaultMetadata);
                    return token;
                }
            };

            _provider.EventFinder = (type, o, arg3) =>
            {
                ++invokeCount;
                type.ShouldEqual(targetType);
                member.ShouldEqual(o);
                arg3.ShouldEqual(DefaultMetadata);
                return result;
            };

            var observer = ObservationManager.TryGetMemberObserver(targetType, member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldUseMemberManager1()
        {
            const string memberName = "Test";
            var set = new HashSet<string>();
            var flags = MemberFlags.Attached | MemberFlags.Public;
            var target = new object();
            var listener = new TestWeakEventListener();
            var token = ActionToken.NoDo;
            var tryObserveCount = 0;

            var targetType = typeof(string);
            var member = new TestAccessorMemberInfo { Name = memberName, MemberFlags = flags };
            var result = new TestEventInfo
            {
                TryObserve = (o, l, arg3) =>
                {
                    ++tryObserveCount;
                    o.ShouldEqual(target);
                    l.ShouldEqual(listener);
                    arg3.ShouldEqual(DefaultMetadata);
                    return token;
                }
            };

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, t, m, f, r, meta) =>
                {
                    set.Add((string)r);
                    if (r.Equals(memberName + BindingInternalConstant.ChangeEventPostfix))
                        return result;
                    return default;
                }
            });

            var observer = ObservationManager.TryGetMemberObserver(targetType, member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            set.Count.ShouldEqual(2);
            set.Contains(memberName + BindingInternalConstant.ChangedEventPostfix).ShouldBeTrue();
            set.Contains(memberName + BindingInternalConstant.ChangeEventPostfix).ShouldBeTrue();
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldUseMemberManager2()
        {
            const string memberName = nameof(TryGetMemberObserverShouldUseMemberManager2);
            var member = typeof(EventMemberObserverProviderTest).GetMethod(memberName)!;
            var set = new HashSet<string>();
            var flags = MemberFlags.InstanceAll;
            var target = new object();
            var listener = new TestWeakEventListener();
            var token = ActionToken.NoDo;
            var tryObserveCount = 0;

            var targetType = typeof(string);
            var result = new TestEventInfo
            {
                TryObserve = (o, l, arg3) =>
                {
                    ++tryObserveCount;
                    o.ShouldEqual(target);
                    l.ShouldEqual(listener);
                    arg3.ShouldEqual(DefaultMetadata);
                    return token;
                }
            };

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, t, m, f, r, meta) =>
                {
                    f.ShouldEqual(flags);
                    set.Add((string)r);
                    if (r.Equals(memberName + BindingInternalConstant.ChangeEventPostfix))
                        return result;
                    return default;
                }
            });

            var observer = ObservationManager.TryGetMemberObserver(targetType, member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            set.Count.ShouldEqual(2);
            set.Contains(memberName + BindingInternalConstant.ChangedEventPostfix).ShouldBeTrue();
            set.Contains(memberName + BindingInternalConstant.ChangeEventPostfix).ShouldBeTrue();
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMemberObserverShouldUseMemberManager3(bool isStatic)
        {
            var member = "EventMember";
            var set = new HashSet<string>();
            var flags = isStatic ? MemberFlags.StaticAll : MemberFlags.InstanceAll;
            var target = new object();
            var listener = new TestWeakEventListener();
            var token = ActionToken.NoDo;
            var tryObserveCount = 0;

            var targetType = isStatic ? typeof(Enumerable) : typeof(string);
            var result = new TestEventInfo
            {
                TryObserve = (o, l, arg3) =>
                {
                    ++tryObserveCount;
                    o.ShouldEqual(target);
                    l.ShouldEqual(listener);
                    arg3.ShouldEqual(DefaultMetadata);
                    return token;
                }
            };

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, t, m, f, r, meta) =>
                {
                    f.ShouldEqual(flags);
                    set.Add((string)r);
                    if (r.Equals(member))
                        return result;
                    return default;
                }
            });

            var observer = ObservationManager.TryGetMemberObserver(targetType, member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            set.Count.ShouldEqual(1);
            set.Contains(member).ShouldBeTrue();
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);
    }
}