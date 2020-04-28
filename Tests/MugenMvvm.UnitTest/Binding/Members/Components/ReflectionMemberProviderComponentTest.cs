using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Observers;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class ReflectionMemberProviderComponentTest : UnitTestBase
    {
        #region Fields

        private readonly int _field;
        private static int FieldStatic;

        #endregion

        #region Properties

        public static object PropertyStatic { get; set; }

        #endregion

        #region Events

        public event Action Event;

        public static event Action EventStatic;

        #endregion

        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnStaticMethods()
        {
            var component = new ReflectionMemberProviderComponent();
            var items = typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(Enumerable.FirstOrDefault))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(typeof(Enumerable), nameof(Enumerable.FirstOrDefault), DefaultMetadata).ToArray();
            members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnStaticFields()
        {
            var component = new ReflectionMemberProviderComponent();
            var items = GetType()
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(FieldStatic))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(GetType(), nameof(FieldStatic), DefaultMetadata).ToArray();
            members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnStaticProperties()
        {
            var component = new ReflectionMemberProviderComponent();
            var items = GetType()
                .GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(PropertyStatic))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(GetType(), nameof(PropertyStatic), DefaultMetadata).ToArray();
            members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMembersShouldReturnStaticEvents(bool canObserve)
        {
            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg3, arg4) =>
                {
                    if (canObserve)
                        return new MemberObserver((o1, o2, listener, context) => new ActionToken(), this);
                    return default;
                }
            });

            var component = new ReflectionMemberProviderComponent(observerProvider);
            var items = GetType()
                .GetEvents(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(EventStatic))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(GetType(), nameof(EventStatic), DefaultMetadata).ToArray();

            if (canObserve)
                members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
            else
                members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldNotContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnInstanceFields()
        {
            var component = new ReflectionMemberProviderComponent();
            var items = GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(_field))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(GetType(), nameof(_field), DefaultMetadata).ToArray();
            members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnInstanceProperties()
        {
            var component = new ReflectionMemberProviderComponent();
            var items = typeof(List<object>)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(List<object>.Count))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(typeof(List<object>), nameof(List<object>.Count), DefaultMetadata).ToArray();
            members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMembersShouldReturnInstanceEvents(bool canObserve)
        {
            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg3, arg4) =>
                {
                    if (canObserve)
                        return new MemberObserver((o1, o2, listener, context) => new ActionToken(), this);
                    return default;
                }
            });

            var component = new ReflectionMemberProviderComponent(observerProvider);
            var items = GetType()
                .GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(Event))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(GetType(), nameof(Event), DefaultMetadata).ToArray();

            if (canObserve)
                members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
            else
                members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldNotContain(items);
        }

        [Fact]
        public void TryGetMembersShouldReturnInstanceMethods()
        {
            var component = new ReflectionMemberProviderComponent();
            var items = typeof(List<object>)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info => info.Name == nameof(List<object>.Remove))
                .ToArray();
            items.ShouldNotBeEmpty();
            var members = component.TryGetMembers(typeof(List<object>), nameof(List<object>.Remove), DefaultMetadata).ToArray();
            members.Select(info => (MemberInfo)info.UnderlyingMember!).ShouldContain(items);
        }

        #endregion
    }
}