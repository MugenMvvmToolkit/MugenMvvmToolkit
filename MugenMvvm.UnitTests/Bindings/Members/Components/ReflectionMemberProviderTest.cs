#pragma warning disable CS0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class ReflectionMemberProviderTest : UnitTestBase
    {
        private static int FieldStatic;
        private readonly int _field;

        private readonly ObservationManager _observationManager;
        private readonly MemberManager _memberManager;
        private readonly ReflectionMemberProvider _provider;

        public ReflectionMemberProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _observationManager = new ObservationManager(ComponentCollectionManager);
            _memberManager = new MemberManager(ComponentCollectionManager);
            _provider = new ReflectionMemberProvider(_observationManager);
            _memberManager.AddComponent(_provider);
        }

        [Fact]
        public void ShouldCacheMembers()
        {
            _observationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) => { return new MemberObserver((o1, o2, listener, context) => new ActionToken(), this); }
            });

            var m1 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), BindingInternalConstant.IndexerGetterName, MemberType.Method, DefaultMetadata)
                              .Item;
            var m2 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), BindingInternalConstant.IndexerGetterName, MemberType.Method, DefaultMetadata)
                              .Item;
            m1.ShouldNotBeNull();
            m1.ShouldEqual(m2);

            m1 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), nameof(EventStatic), MemberType.Event, DefaultMetadata).Item;
            m2 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), nameof(EventStatic), MemberType.Event, DefaultMetadata).Item;
            m1.ShouldNotBeNull();
            m1.ShouldEqual(m2);

            m1 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), nameof(FieldStatic), MemberType.Accessor, DefaultMetadata).Item;
            m2 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), nameof(FieldStatic), MemberType.Accessor, DefaultMetadata).Item;
            m1.ShouldNotBeNull();
            m1.ShouldEqual(m2);

            m1 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), nameof(PropertyStatic), MemberType.Accessor, DefaultMetadata).Item;
            m2 = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), nameof(PropertyStatic), MemberType.Accessor, DefaultMetadata).Item;
            m1.ShouldNotBeNull();
            m1.ShouldEqual(m2);
        }

        [Fact]
        public void TryGetMembersShouldReturnIndexerMethods()
        {
            var member = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), BindingInternalConstant.IndexerGetterName, MemberType.Method,
                DefaultMetadata).Item;
            member!.UnderlyingMember.ShouldEqual(typeof(ReflectionMemberProviderTest).GetProperty("Item")!.GetMethod);

            member = _provider.TryGetMembers(_memberManager, typeof(ReflectionMemberProviderTest), BindingInternalConstant.IndexerSetterName, MemberType.Method, DefaultMetadata)
                              .Item;
            member!.UnderlyingMember.ShouldEqual(typeof(ReflectionMemberProviderTest).GetProperty("Item")!.SetMethod);

            member = _provider.TryGetMembers(_memberManager, typeof(string), BindingInternalConstant.IndexerGetterName, MemberType.Method, DefaultMetadata).Item;
            member!.UnderlyingMember.ShouldEqual(typeof(string).GetProperty("Chars")!.GetMethod);
            _provider.TryGetMembers(_memberManager, typeof(string), BindingInternalConstant.IndexerSetterName, MemberType.Method, DefaultMetadata).IsEmpty.ShouldBeTrue();

            member = _provider.TryGetMembers(_memberManager, typeof(ItemOrArray<object>), BindingInternalConstant.IndexerGetterName, MemberType.Method, DefaultMetadata).Item;
            member!.UnderlyingMember.ShouldEqual(typeof(ItemOrArray<object>).GetProperty(InternalConstant.CustomIndexerName)!.GetMethod);
            _provider.TryGetMembers(_memberManager, typeof(ItemOrArray<object>), BindingInternalConstant.IndexerSetterName, MemberType.Method, DefaultMetadata).IsEmpty
                     .ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldReturnInstanceFields()
        {
            var component = new ReflectionMemberProvider();
            var items = GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(_field))
                        .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(null!, GetType(), nameof(_field), MemberType.Accessor, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnInstanceMethods()
        {
            var items = typeof(List<object>)
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(List<object>.Remove))
                        .ToArray();
            items.ShouldNotBeEmpty();
            var members = _provider.TryGetMembers(_memberManager, typeof(List<object>), nameof(List<object>.Remove), MemberType.Method, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnInstanceProperties()
        {
            var items = typeof(List<object>)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(List<object>.Count))
                        .ToArray();
            items.ShouldNotBeEmpty();
            var members = _provider.TryGetMembers(_memberManager, typeof(List<object>), nameof(List<object>.Count), MemberType.Accessor, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnStaticFields()
        {
            var items = GetType()
                        .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(FieldStatic))
                        .ToArray();
            items.ShouldNotBeEmpty();
            _provider.TryGetMembers(_memberManager, GetType(), nameof(FieldStatic), MemberType.Event, DefaultMetadata).IsEmpty.ShouldBeTrue();
            var members = _provider.TryGetMembers(_memberManager, GetType(), nameof(FieldStatic), MemberType.Accessor, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnStaticMethods()
        {
            var items = typeof(Enumerable)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(Enumerable.FirstOrDefault))
                        .ToArray();
            items.ShouldNotBeEmpty();

            _provider.TryGetMembers(_memberManager, typeof(Enumerable), nameof(Enumerable.FirstOrDefault), MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();
            var members = _provider.TryGetMembers(_memberManager, typeof(Enumerable), nameof(Enumerable.FirstOrDefault), MemberType.Method, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnStaticProperties()
        {
            var items = GetType()
                        .GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(PropertyStatic))
                        .ToArray();
            items.ShouldNotBeEmpty();
            var members = _provider.TryGetMembers(_memberManager, GetType(), nameof(PropertyStatic), MemberType.Accessor, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        public object this[int index]
        {
            get => this;
            set { }
        }

        public static object? PropertyStatic { get; set; }

        public event Action? Event;

        public static event Action? EventStatic;

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMembersShouldReturnStaticEvents(bool canObserve)
        {
            _observationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) =>
                {
                    if (canObserve)
                        return new MemberObserver((o1, o2, listener, context) => new ActionToken(), this);
                    return default;
                }
            });

            var items = GetType()
                        .GetEvents(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(EventStatic))
                        .ToArray();
            items.ShouldNotBeEmpty();
            _provider.TryGetMembers(_memberManager, GetType(), nameof(EventStatic), MemberType.Method, DefaultMetadata).IsEmpty.ShouldBeTrue();
            var members = _provider.TryGetMembers(_memberManager, GetType(), nameof(EventStatic), MemberType.Event, DefaultMetadata).AsList();

            if (canObserve)
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
            else
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldNotContain(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMembersShouldReturnInstanceEvents(bool canObserve)
        {
            _observationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) =>
                {
                    if (canObserve)
                        return new MemberObserver((o1, o2, listener, context) => new ActionToken(), this);
                    return default;
                }
            });

            var items = GetType()
                        .GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(info => info.Name == nameof(Event))
                        .ToArray();
            items.ShouldNotBeEmpty();
            var members = _provider.TryGetMembers(_memberManager, GetType(), nameof(Event), MemberType.Event, DefaultMetadata).AsList();

            if (canObserve)
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
            else
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldNotContain(items);
        }
    }
}