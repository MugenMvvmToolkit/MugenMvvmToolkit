using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class AttachedDynamicMemberProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnNullResult()
        {
            var component = new AttachedDynamicMemberProvider();
            component.TryGetMembers(null!, typeof(object), string.Empty, MemberType.All, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void TryGetMembersShouldRegisterUnregisterMembers(int count, bool clear)
        {
            var requestType = typeof(string);
            var name = "test";
            var memberType = MemberType.Method;

            var invalidateCount = 0;
            var hasCache = new TestHasCache
            {
                Invalidate = (o, arg3) => ++invalidateCount
            };
            var owner = new MemberManager();
            var component = new AttachedDynamicMemberProvider();
            owner.AddComponent(component);
            owner.Components.Add(hasCache);
            var list = new List<IMemberInfo>();
            var delegates = new List<Func<Type, string, MemberType, IReadOnlyMetadataContext?, IMemberInfo?>>();
            for (var i = 0; i < count; i++)
            {
                Func<Type, string, MemberType, IReadOnlyMetadataContext?, IMemberInfo?> func = (type, s, t, arg3) =>
                {
                    memberType.ShouldEqual(t);
                    type.ShouldEqual(requestType);
                    name.ShouldEqual(s);
                    arg3.ShouldEqual(DefaultMetadata);
                    var info = new TestAccessorMemberInfo();
                    list.Add(info);
                    return info;
                };
                delegates.Add(func);
                component.Register(func);
            }

            invalidateCount.ShouldEqual(count);
            component.TryGetMembers(null!, requestType, name, memberType, DefaultMetadata).AsList().ShouldEqual(list);
            list.Count.ShouldEqual(count);

            invalidateCount = 0;
            if (clear)
                component.Clear();
            else
            {
                foreach (var @delegate in delegates)
                    component.Unregister(@delegate);
            }

            component.TryGetMembers(null!, typeof(object), string.Empty, memberType, DefaultMetadata).IsEmpty.ShouldBeTrue();
            invalidateCount.ShouldEqual(clear ? 1 : count);
        }

        #endregion
    }
}