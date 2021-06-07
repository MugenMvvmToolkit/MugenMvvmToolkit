using System;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
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
    public class FieldAccessorMemberInfoTest : UnitTestBase
    {
        public static readonly string? InitOnlyStaticField = "";

        [NonObservable]
        public static string? NonObservableStaticField;

        public static string? Field1Static;
        public static int Field2Static;

        public readonly string? InitOnlyField = "";

        [NonObservable]
        public string? NonObservableField;

        public string? Field1;
        public int Field2;

        public FieldAccessorMemberInfoTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(ObservationManager));
            RegisterDisposeToken(WithGlobalService(MemberManager));
            RegisterDisposeToken(WithGlobalService(ReflectionManager));
        }

        [Theory]
        [InlineData(nameof(Field1), false)]
        [InlineData(nameof(Field2), false)]
        [InlineData(nameof(NonObservableField), true)]
        [InlineData(nameof(InitOnlyField), true)]
        public void ConstructorShouldInitializeMember(string fieldName, bool nonObservable)
        {
            var fieldInfo = GetType().GetField(fieldName)!;
            fieldInfo.ShouldNotBeNull();
            var reflectedType = typeof(object);
            var name = fieldName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = ActionToken.FromDelegate((o, o1) => { });
            var count = 0;
            FieldAccessorMemberInfo? memberInfo = null;

            var memberObserver = new MemberObserver((target, member, listener, meta) =>
            {
                ++count;
                target.ShouldEqual(this);
                member.ShouldEqual(fieldInfo);
                listener.ShouldEqual(testEventListener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, fieldInfo);

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

            memberInfo = new FieldAccessorMemberInfo(name, fieldInfo, reflectedType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(fieldInfo.FieldType);
            memberInfo.DeclaringType.ShouldEqual(fieldInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(fieldInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Accessor);
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Instance | (nonObservable ? MemberFlags.NonObservable : default));

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
            observerRequestCount.ShouldEqual(1);

            if (fieldName == nameof(Field1))
            {
                Field1 = "test";
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Field1);
                memberInfo.SetValue(this, nameof(Field1), DefaultMetadata);
                Field1.ShouldEqual(nameof(Field1));
            }
            else if (fieldName == nameof(Field2))
            {
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Field2);
                memberInfo.SetValue(this, int.MaxValue, DefaultMetadata);
                Field2.ShouldEqual(int.MaxValue);
            }
            else if (fieldName == nameof(InitOnlyField))
            {
                memberInfo.CanWrite.ShouldBeFalse();
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(InitOnlyField);
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, "", DefaultMetadata));
            }
        }

        [Theory]
        [InlineData(nameof(Field1Static), false)]
        [InlineData(nameof(Field2Static), false)]
        [InlineData(nameof(NonObservableStaticField), true)]
        [InlineData(nameof(InitOnlyStaticField), true)]
        public void ConstructorShouldInitializeStaticMember(string fieldName, bool nonObservable)
        {
            var fieldInfo = GetType().GetField(fieldName)!;
            fieldInfo.ShouldNotBeNull();
            var reflectedType = typeof(object);
            var name = fieldName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = ActionToken.FromDelegate((o, o1) => { });
            var count = 0;
            FieldAccessorMemberInfo? memberInfo = null;

            var memberObserver = new MemberObserver((target, member, listener, meta) =>
            {
                ++count;
                target.ShouldEqual(this);
                member.ShouldEqual(fieldInfo);
                listener.ShouldEqual(testEventListener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, fieldInfo);

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

            memberInfo = new FieldAccessorMemberInfo(name, fieldInfo, reflectedType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(fieldInfo.FieldType);
            memberInfo.DeclaringType.ShouldEqual(fieldInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(fieldInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Accessor);
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Static | MemberFlags.Public | (nonObservable ? MemberFlags.NonObservable : default));

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
            observerRequestCount.ShouldEqual(1);

            if (fieldName == nameof(Field1Static))
            {
                Field1Static = "test";
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(Field1Static);
                memberInfo.SetValue(null, nameof(Field1Static), DefaultMetadata);
                Field1Static.ShouldEqual(nameof(Field1Static));
            }
            else if (fieldName == nameof(Field2Static))
            {
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(Field2Static);
                memberInfo.SetValue(null, int.MaxValue, DefaultMetadata);
                Field2Static.ShouldEqual(int.MaxValue);
            }
            else if (fieldName == nameof(InitOnlyStaticField))
            {
                memberInfo.CanWrite.ShouldBeFalse();
                memberInfo.GetValue(null, DefaultMetadata).ShouldEqual(InitOnlyStaticField);
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, "", DefaultMetadata));
            }
        }

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);
    }
}