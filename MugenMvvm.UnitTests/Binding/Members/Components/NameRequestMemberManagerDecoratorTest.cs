﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Members.Components
{
    public class NameRequestMemberManagerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldIgnoreNotSupportedRequest()
        {
            var manager = new MemberManager();
            var component = new NameRequestMemberManagerDecorator();
            manager.AddComponent(component);
            component.TryGetMembers(manager, typeof(object), MemberType.All, MemberFlags.All, "", DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.TryGetMembers(manager, typeof(object), MemberType.All, MemberFlags.All, this, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldUseSelector()
        {
            var type = GetType();
            var memberType = MemberType.All;
            var memberFlags = MemberFlags.Instance;
            var request = "";
            var selectorCount = 0;
            var providerCount = 0;
            var members = new[] {new TestAccessorMemberInfo(), new TestAccessorMemberInfo()};

            var manager = new MemberManager();
            var selector = new TestMemberManagerComponent(manager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++selectorCount;
                    ((IEnumerable<IMemberInfo>) r).SequenceEqual(members).ShouldBeTrue();
                    type.ShouldEqual(t);
                    memberType.ShouldEqual(m);
                    memberFlags.ShouldEqual(f);
                    meta.ShouldEqual(DefaultMetadata);
                    return members;
                }
            };
            var provider = new TestMemberProviderComponent(manager)
            {
                TryGetMembers = (t, s, types, arg3) =>
                {
                    ++providerCount;
                    types.ShouldEqual(memberType);
                    type.ShouldEqual(t);
                    s.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    return members;
                }
            };
            var component = new NameRequestMemberManagerDecorator();
            manager.AddComponent(selector);
            manager.AddComponent(provider);
            manager.AddComponent(component);

            manager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).AsList().SequenceEqual(members).ShouldBeTrue();
            selectorCount.ShouldEqual(1);
            providerCount.ShouldEqual(1);
        }

        #endregion
    }
}