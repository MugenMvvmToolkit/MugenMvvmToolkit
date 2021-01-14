using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class PropertyAccessorMemberInfoTest : UnitTestBase
    {
        public static string? Property1Static { get; set; }

        public static int Property2Static { get; set; }

        public static string ReadOnlyPropertyStatic => nameof(ReadOnlyPropertyStatic);

        public static string WriteOnlyPropertyStatic
        {
            set { ; }
        }

        public string? Property1 { get; set; }

        public int Property2 { get; set; }

        public string ReadOnlyProperty => nameof(ReadOnlyProperty);

        public string WriteOnlyProperty
        {
            set { ; }
        }

        [Theory]
        [InlineData(nameof(Property1))]
        [InlineData(nameof(Property2))]
        [InlineData(nameof(ReadOnlyProperty))]
        [InlineData(nameof(WriteOnlyProperty))]
        public void ConstructorShouldInitializeMember(string fieldName)
        {
            var propertyInfo = GetType().GetProperty(fieldName)!;
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
            using var t = MugenService.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) =>
                {
                    ++observerRequestCount;
                    o.ShouldEqual(memberInfo);
                    arg4.ShouldEqual(DefaultMetadata);
                    type.ShouldEqual(reflectedType);
                    return memberObserver;
                }
            });

            memberInfo = new PropertyAccessorMemberInfo(name, propertyInfo, reflectedType);
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
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(ReadOnlyProperty);
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, nameof(Property1), DefaultMetadata));
            }
            else if (fieldName == nameof(WriteOnlyProperty))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeFalse();
                memberInfo.SetValue(this, "", DefaultMetadata);
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

        [Theory]
        [InlineData(nameof(Property1Static))]
        [InlineData(nameof(Property2Static))]
        [InlineData(nameof(ReadOnlyPropertyStatic))]
        [InlineData(nameof(WriteOnlyPropertyStatic))]
        public void ConstructorShouldInitializeStaticMember(string fieldName)
        {
            var propertyInfo = GetType().GetProperty(fieldName)!;
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
            using var t = MugenService.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) =>
                {
                    ++observerRequestCount;
                    o.ShouldEqual(memberInfo);
                    arg4.ShouldEqual(DefaultMetadata);
                    type.ShouldEqual(reflectedType);
                    return memberObserver;
                }
            });

            memberInfo = new PropertyAccessorMemberInfo(name, propertyInfo, reflectedType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(propertyInfo.PropertyType);
            memberInfo.DeclaringType.ShouldEqual(propertyInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(propertyInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Accessor);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Static);

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
            observerRequestCount.ShouldEqual(1);

            if (fieldName == nameof(Property1Static))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeTrue();
                Property1Static = "test";
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(Property1Static);
                memberInfo.SetValue(null, nameof(Property1Static), DefaultMetadata);
                Property1Static.ShouldEqual(nameof(Property1Static));
            }
            else if (fieldName == nameof(ReadOnlyPropertyStatic))
            {
                memberInfo.CanWrite.ShouldBeFalse();
                memberInfo.CanRead.ShouldBeTrue();
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(ReadOnlyPropertyStatic);
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(null, nameof(Property1Static), DefaultMetadata));
            }
            else if (fieldName == nameof(WriteOnlyPropertyStatic))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeFalse();
                memberInfo.SetValue(null, "", DefaultMetadata);
                ShouldThrow<InvalidOperationException>(() => memberInfo.GetValue(null, DefaultMetadata));
            }
            else
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeTrue();
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(Property2Static);
                memberInfo.SetValue(null, int.MaxValue, DefaultMetadata);
                Property2Static.ShouldEqual(int.MaxValue);
            }
        }
    }
}