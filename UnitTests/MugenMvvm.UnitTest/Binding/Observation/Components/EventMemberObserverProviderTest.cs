using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observation.Components
{
    public class EventMemberObserverProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest()
        {
            var component = new EventMemberObserverProvider();
            component.TryGetMemberObserver(null!, typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMemberObserverShouldUseEventFinder1()
        {
            var target = new object();
            var listener = new TestWeakEventListener();
            var token = ActionToken.NoDoToken;
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
            var component = new EventMemberObserverProvider
            {
                EventFinder = (type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(targetType);
                    member.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };

            var observer = component.TryGetMemberObserver(null!, targetType, member, DefaultMetadata);
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
            var token = ActionToken.NoDoToken;
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
            var component = new EventMemberObserverProvider
            {
                EventFinder = (type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(targetType);
                    member.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };

            var observer = component.TryGetMemberObserver(null!, targetType, member, DefaultMetadata);
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
            var token = ActionToken.NoDoToken;
            var tryObserveCount = 0;

            var targetType = typeof(string);
            var member = new TestAccessorMemberInfo {Name = memberName, AccessModifiers = flags};
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

            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    set.Add((string) r);
                    if (r.Equals(memberName + BindingInternalConstant.ChangeEventPostfix))
                        return result;
                    return default;
                }
            });

            var component = new EventMemberObserverProvider(memberManager);

            var observer = component.TryGetMemberObserver(null!, targetType, member, DefaultMetadata);
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
            var token = ActionToken.NoDoToken;
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

            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    f.ShouldEqual(flags);
                    set.Add((string) r);
                    if (r.Equals(memberName + BindingInternalConstant.ChangeEventPostfix))
                        return result;
                    return default;
                }
            });

            var component = new EventMemberObserverProvider(memberManager);
            var observer = component.TryGetMemberObserver(null!, targetType, member, DefaultMetadata);
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
            var token = ActionToken.NoDoToken;
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

            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    f.ShouldEqual(flags);
                    set.Add((string) r);
                    if (r.Equals(member))
                        return result;
                    return default;
                }
            });

            var component = new EventMemberObserverProvider(memberManager);
            var observer = component.TryGetMemberObserver(null!, targetType, member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            set.Count.ShouldEqual(1);
            set.Contains(member).ShouldBeTrue();
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        #endregion
    }
}