#pragma warning disable CS0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class ReflectionMemberProviderTest : UnitTestBase
    {
        #region Fields

        private readonly int _field;
        private static int FieldStatic;

        #endregion

        #region Properties

        public static object? PropertyStatic { get; set; }

        #endregion

        #region Events

        public event Action? Event;

        public static event Action? EventStatic;

        #endregion

        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnStaticMethods()
        {
            var component = new ReflectionMemberProvider();
            var items = typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(Enumerable.FirstOrDefault))
                .ToArray();
            items.ShouldNotBeEmpty();

            component.TryGetMembers(null!, typeof(Enumerable), nameof(Enumerable.FirstOrDefault), MemberType.Accessor, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            var members = component.TryGetMembers(null!, typeof(Enumerable), nameof(Enumerable.FirstOrDefault), MemberType.Method, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnStaticFields()
        {
            var component = new ReflectionMemberProvider();
            var items = GetType()
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(FieldStatic))
                .ToArray();
            items.ShouldNotBeEmpty();
            component.TryGetMembers(null!, GetType(), nameof(FieldStatic), MemberType.Event, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            var members = component.TryGetMembers(null!, GetType(), nameof(FieldStatic), MemberType.Accessor, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnStaticProperties()
        {
            var component = new ReflectionMemberProvider();
            var items = GetType()
                .GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(PropertyStatic))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(null!, GetType(), nameof(PropertyStatic), MemberType.Accessor, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMembersShouldReturnStaticEvents(bool canObserve)
        {
            var observationManager = new ObservationManager();
            observationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) =>
                {
                    if (canObserve)
                        return new MemberObserver((o1, o2, listener, context) => new ActionToken(), this);
                    return default;
                }
            });

            var component = new ReflectionMemberProvider(observationManager);
            var items = GetType()
                .GetEvents(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(EventStatic))
                .ToArray();
            items.ShouldNotBeEmpty();
            component.TryGetMembers(null!, GetType(), nameof(EventStatic), MemberType.Method, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            var members = component.TryGetMembers(null!, GetType(), nameof(EventStatic), MemberType.Event, DefaultMetadata).AsList();

            if (canObserve)
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
            else
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldNotContain(items);
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
        public void TryGetMembersShouldReturnInstanceProperties()
        {
            var component = new ReflectionMemberProvider();
            var items = typeof(List<object>)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(List<object>.Count))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(null!, typeof(List<object>), nameof(List<object>.Count), MemberType.Accessor, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMembersShouldReturnInstanceEvents(bool canObserve)
        {
            var observationManager = new ObservationManager();
            observationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) =>
                {
                    if (canObserve)
                        return new MemberObserver((o1, o2, listener, context) => new ActionToken(), this);
                    return default;
                }
            });

            var component = new ReflectionMemberProvider(observationManager);
            var items = GetType()
                .GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(Event))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(null!, GetType(), nameof(Event), MemberType.Event, DefaultMetadata).AsList();

            if (canObserve)
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
            else
                members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldNotContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnInstanceMethods()
        {
            var component = new ReflectionMemberProvider();
            var items = typeof(List<object>)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(List<object>.Remove))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(null!, typeof(List<object>), nameof(List<object>.Remove), MemberType.Method, DefaultMetadata).AsList();
            members.Select(info => (MemberInfo) info.UnderlyingMember!).ShouldContain(items);
        }

        #endregion
    }
}