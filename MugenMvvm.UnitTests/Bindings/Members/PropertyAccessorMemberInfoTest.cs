using System;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    [Collection(SharedContext)]
    public class PropertyAccessorMemberInfoTest : UnitTestBase
    {
        public PropertyAccessorMemberInfoTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(ObservationManager));
            RegisterDisposeToken(WithGlobalService(ReflectionManager));
        }

        public static string? Property1Static { get; set; }

        public static int Property2Static { get; set; }

        [NonObservable]
        public static int NonObservableStaticProperty { get; set; }

        public static string ReadOnlyPropertyStaticGenerated { get; } = nameof(ReadOnlyPropertyStaticGenerated);

        public static string ReadOnlyPropertyStatic => nameof(ReadOnlyPropertyStatic);

        public static string WriteOnlyPropertyStatic
        {
            set { ; }
        }

        public string? Property1 { get; set; }

        public int Property2 { get; set; }

        [NonObservable]
        public int NonObservableProperty { get; set; }

        public string ReadOnlyPropertyGenerated { get; } = nameof(ReadOnlyPropertyGenerated);

        public string ReadOnlyProperty => nameof(ReadOnlyProperty);

        public string WriteOnlyProperty
        {
            set { ; }
        }

        [Theory]
        [InlineData(nameof(Property1), false)]
        [InlineData(nameof(Property2), false)]
        [InlineData(nameof(ReadOnlyProperty), false)]
        [InlineData(nameof(WriteOnlyProperty), false)]
        [InlineData(nameof(ReadOnlyPropertyGenerated), true)]
        [InlineData(nameof(NonObservableProperty), true)]
        public void ConstructorShouldInitializeMember(string fieldName, bool nonObservable)
        {
            var propertyInfo = GetType().GetProperty(fieldName)!;
            propertyInfo.ShouldNotBeNull();
            var reflectedType = typeof(object);
            var name = fieldName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = ActionToken.FromDelegate((o, o1) => { });
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
            ObservationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (_, type, o, arg4) =>
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
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Instance | (nonObservable ? MemberFlags.NonObservable : default));

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
            else if (fieldName == nameof(ReadOnlyPropertyGenerated))
            {
                memberInfo.CanWrite.ShouldBeFalse();
                memberInfo.CanRead.ShouldBeTrue();
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(ReadOnlyPropertyGenerated);
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, nameof(Property1), DefaultMetadata));
            }
            else if (fieldName == nameof(Property2))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeTrue();
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Property2);
                memberInfo.SetValue(this, int.MaxValue, DefaultMetadata);
                Property2.ShouldEqual(int.MaxValue);
            }
        }

        [Theory]
        [InlineData(nameof(Property1Static), false)]
        [InlineData(nameof(Property2Static), false)]
        [InlineData(nameof(ReadOnlyPropertyStatic), false)]
        [InlineData(nameof(WriteOnlyPropertyStatic), false)]
        [InlineData(nameof(ReadOnlyPropertyStaticGenerated), true)]
        [InlineData(nameof(NonObservableStaticProperty), true)]
        public void ConstructorShouldInitializeStaticMember(string fieldName, bool nonObservable)
        {
            var propertyInfo = GetType().GetProperty(fieldName)!;
            propertyInfo.ShouldNotBeNull();
            var reflectedType = typeof(object);
            var name = fieldName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = ActionToken.FromDelegate((o, o1) => { });
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
            ObservationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (_, type, o, arg4) =>
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
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Static | (nonObservable ? MemberFlags.NonObservable : default));

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
            else if (fieldName == nameof(ReadOnlyPropertyStaticGenerated))
            {
                memberInfo.CanWrite.ShouldBeFalse();
                memberInfo.CanRead.ShouldBeTrue();
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(ReadOnlyPropertyStaticGenerated);
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(null, nameof(Property1Static), DefaultMetadata));
            }
            else if (fieldName == nameof(WriteOnlyPropertyStatic))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeFalse();
                memberInfo.SetValue(null, "", DefaultMetadata);
                ShouldThrow<InvalidOperationException>(() => memberInfo.GetValue(null, DefaultMetadata));
            }
            else if (fieldName == nameof(Property2Static))
            {
                memberInfo.CanWrite.ShouldBeTrue();
                memberInfo.CanRead.ShouldBeTrue();
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(Property2Static);
                memberInfo.SetValue(null, int.MaxValue, DefaultMetadata);
                Property2Static.ShouldEqual(int.MaxValue);
            }
        }

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);
    }
}