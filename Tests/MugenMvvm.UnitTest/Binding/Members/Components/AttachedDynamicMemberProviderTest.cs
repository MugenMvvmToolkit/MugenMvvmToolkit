using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class AttachedDynamicMemberProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnNullResult()
        {
            var component = new AttachedDynamicMemberProvider();
            component.TryGetMembers(typeof(object), string.Empty, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
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

            var invalidateCount = 0;
            var hasCache = new TestHasCache
            {
                Invalidate = (o, type, arg3) => ++invalidateCount
            };
            var owner = new MemberManager();
            var component = new AttachedDynamicMemberProvider();
            owner.AddComponent(component);
            owner.Components.Add(hasCache);
            var list = new List<IMemberInfo>();
            var delegates = new List<Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?>>();
            for (var i = 0; i < count; i++)
            {
                Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?> func = (type, s, arg3) =>
                {
                    type.ShouldEqual(requestType);
                    name.ShouldEqual(s);
                    arg3.ShouldEqual(DefaultMetadata);
                    var info = new TestMemberAccessorInfo();
                    list.Add(info);
                    return info;
                };
                delegates.Add(func);
                component.Register(func);
            }

            invalidateCount.ShouldEqual(count);
            component.TryGetMembers(requestType, name, DefaultMetadata).AsList().SequenceEqual(list).ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).AsList().SequenceEqual(list).ShouldBeTrue();
            list.Count.ShouldEqual(count);

            invalidateCount = 0;
            if (clear)
                component.Clear();
            else
            {
                foreach (var @delegate in delegates)
                    component.Unregister(@delegate);
            }

            component.TryGetMembers(typeof(object), string.Empty, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            invalidateCount.ShouldEqual(clear ? 1 : count);
        }

        #endregion
    }
}