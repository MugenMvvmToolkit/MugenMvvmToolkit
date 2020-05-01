﻿using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class EventMemberObserverProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest()
        {
            var component = new EventMemberObserverProviderComponent();
            component.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMemberObserverShouldUseEventFinder1(bool rawRequest)
        {
            var target = new object();
            var listener = new TestEventListener();
            var token = ActionToken.NoDoToken;
            var tryObserveCount = 0;

            var invokeCount = 0;
            var requestType = typeof(string);
            var member = typeof(EventMemberObserverProviderComponentTest).GetMethod(nameof(TryGetMemberObserverShouldUseEventFinder1));
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
            var component = new EventMemberObserverProviderComponent
            {
                EventFinder = (type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(requestType);
                    member.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };

            var observer = rawRequest
                ? component.TryGetMemberObserver(requestType, member, DefaultMetadata)
                : component.TryGetMemberObserver(requestType, new MemberObserverRequest("", member, null), DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMemberObserverShouldUseEventFinder2(bool rawRequest)
        {
            var target = new object();
            var listener = new TestEventListener();
            var token = ActionToken.NoDoToken;
            var tryObserveCount = 0;

            var invokeCount = 0;
            var requestType = typeof(string);
            var member = new TestMemberAccessorInfo();
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
            var component = new EventMemberObserverProviderComponent
            {
                EventFinder = (type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(requestType);
                    member.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };

            var observer = rawRequest
                ? component.TryGetMemberObserver(requestType, member, DefaultMetadata)
                : component.TryGetMemberObserver(requestType, new MemberObserverRequest("", null, null, member), DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMemberObserverShouldUseMemberManager1(bool rawRequest)
        {
            const string memberName = "Test";
            var set = new HashSet<string>();
            var flags = MemberFlags.Attached | MemberFlags.Public;
            var target = new object();
            var listener = new TestEventListener();
            var token = ActionToken.NoDoToken;
            var tryObserveCount = 0;

            var requestType = typeof(string);
            var member = new TestMemberAccessorInfo {Name = memberName, AccessModifiers = flags};
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
                TryGetMembers = (o, type, arg3) =>
                {
                    var managerRequest = (MemberManagerRequest) o;
                    set.Add(managerRequest.Name);
                    if (managerRequest.Name == memberName + BindingInternalConstant.ChangeEventPostfix)
                        return result;
                    return default;
                }
            });

            var component = new EventMemberObserverProviderComponent(memberManager);

            var observer = rawRequest
                ? component.TryGetMemberObserver(requestType, member, DefaultMetadata)
                : component.TryGetMemberObserver(requestType, new MemberObserverRequest("", null, null, member), DefaultMetadata);
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
        public void TryGetMemberObserverShouldUseMemberManager2(bool rawRequest)
        {
            const string memberName = nameof(TryGetMemberObserverShouldUseMemberManager2);
            var set = new HashSet<string>();
            var flags = MemberFlags.Attached | MemberFlags.Public;
            var target = new object();
            var listener = new TestEventListener();
            var token = ActionToken.NoDoToken;
            var tryObserveCount = 0;

            var requestType = typeof(string);
            var member = typeof(EventMemberObserverProviderComponentTest).GetMethod(memberName);
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
                TryGetMembers = (o, type, arg3) =>
                {
                    var managerRequest = (MemberManagerRequest) o;
                    set.Add(managerRequest.Name);
                    if (managerRequest.Name == memberName + BindingInternalConstant.ChangeEventPostfix)
                        return result;
                    return default;
                }
            });

            var component = new EventMemberObserverProviderComponent(memberManager);

            var observer = rawRequest
                ? component.TryGetMemberObserver(requestType, member, DefaultMetadata)
                : component.TryGetMemberObserver(requestType, new MemberObserverRequest("", member, null), DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();
            set.Count.ShouldEqual(2);
            set.Contains(memberName + BindingInternalConstant.ChangedEventPostfix).ShouldBeTrue();
            set.Contains(memberName + BindingInternalConstant.ChangeEventPostfix).ShouldBeTrue();
            tryObserveCount.ShouldEqual(0);

            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(token);
            tryObserveCount.ShouldEqual(1);
        }

        #endregion
    }
}