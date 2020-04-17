﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class AttachedDynamicMemberProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnNullResult()
        {
            var component = new AttachedDynamicMemberProviderComponent();
            component.TryGetMembers(typeof(object), string.Empty, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetMembersShouldRegisterUnregisterMembers(int count)
        {
            var requestType = typeof(string);
            var name = "test";

            var invalidateCount = 0;
            var hasCache = new TestHasCache
            {
                Invalidate = (o, type, arg3) => ++invalidateCount
            };
            var owner = new MemberManager();
            var component = new AttachedDynamicMemberProviderComponent();
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
            component.TryGetMembers(requestType, name, DefaultMetadata).ToArray().SequenceEqual(list).ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).ToArray().SequenceEqual(list).ShouldBeTrue();
            list.Count.ShouldEqual(count);

            invalidateCount = 0;
            foreach (var @delegate in delegates)
                component.Unregister(@delegate);
            component.TryGetMembers(typeof(object), string.Empty, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            invalidateCount.ShouldEqual(count);
        }

        #endregion
    }
}