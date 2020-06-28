using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class PropertyAccessorMemberInfoTest : UnitTestBase
    {
        #region Properties

        public string? Property1 { get; set; }

        public int Property2 { get; set; }

        public string ReadOnlyProperty => nameof(ReadOnlyProperty);

        public string WriteOnlyProperty
        {
            set { ; }
        }

        #endregion

        #region Methods

        [Theory]
        [InlineData(nameof(Property1))]
        [InlineData(nameof(Property2))]
        [InlineData(nameof(ReadOnlyProperty))]
        [InlineData(nameof(WriteOnlyProperty))]
        public void ConstructorShouldInitializeMember(string fieldName)
        {
            var propertyInfo = GetType().GetProperty(fieldName);
            propertyInfo.ShouldNotBeNull();
            var reflectedType = typeof(object);
            var name = fieldName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = new ActionToken((o, o1) => { });
            var count = 0;
            PropertyAccessorMemberInfo? memberInfo = null;

            var memberObserver = new MemberObserver((target, member, listener, meta) =>
            {
                ++count;
                target.ShouldEqual(this);
                member.ShouldEqual(propertyInfo);
                listener.ShouldEqual(testEventListener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, propertyInfo);

            var observerRequestCount = 0;
            var observerProvider = new ObservationManager();
            observerProvider.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg3, arg4) =>
                {
                    ++observerRequestCount;
                    o.ShouldEqual(memberInfo);
                    arg3.ShouldEqual(typeof(PropertyAccessorMemberInfo));
                    arg4.ShouldEqual(DefaultMetadata);
                    type.ShouldEqual(reflectedType);
                    return memberObserver;
                }
            });

            var delegateProvider = new ReflectionDelegateProvider();
            delegateProvider.AddComponent(new ExpressionReflectionDelegateProvider());

            memberInfo = new PropertyAccessorMemberInfo(name, propertyInfo, reflectedType, observerProvider, delegateProvider);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(propertyInfo.PropertyType);
            memberInfo.DeclaringType.ShouldEqual(propertyInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(propertyInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Accessor);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
            observerRequestCount.ShouldEqual(1);

            if (fieldName == nameof(Property1))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeTrue();
                Property1 = "test";
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Property1);
                memberInfo.SetValue(this, nameof(Property1), DefaultMetadata);
                Property1.ShouldEqual(nameof(Property1));
            }
            else if (fieldName == nameof(ReadOnlyProperty))
            {
                memberInfo.CanWrite.ShouldBeFalse();
                memberInfo.CanRead.ShouldBeTrue();
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, nameof(Property1), DefaultMetadata));
            }
            else if (fieldName == nameof(WriteOnlyProperty))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeFalse();
                ShouldThrow<InvalidOperationException>(() => memberInfo.GetValue(this, DefaultMetadata));
            }
            else
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeTrue();
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Property2);
                memberInfo.SetValue(this, int.MaxValue, DefaultMetadata);
                Property2.ShouldEqual(int.MaxValue);
            }
        }

        #endregion
    }
}