using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class FieldAccessorMemberInfoTest : UnitTestBase
    {
        public static string? Field1Static;
        public static int Field2Static;

        public string? Field1;
        public int Field2;

        [Theory]
        [InlineData(nameof(Field1))]
        [InlineData(nameof(Field2))]
        public void ConstructorShouldInitializeMember(string fieldName)
        {
            var fieldInfo = GetType().GetField(fieldName)!;
            fieldInfo.ShouldNotBeNull();
            var reflectedType = typeof(object);
            var name = fieldName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = new ActionToken((o, o1) => { });
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

            memberInfo = new FieldAccessorMemberInfo(name, fieldInfo, reflectedType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(fieldInfo.FieldType);
            memberInfo.DeclaringType.ShouldEqual(fieldInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(fieldInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Accessor);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);

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
            else
            {
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Field2);
                memberInfo.SetValue(this, int.MaxValue, DefaultMetadata);
                Field2.ShouldEqual(int.MaxValue);
            }
        }

        [Theory]
        [InlineData(nameof(Field1Static))]
        [InlineData(nameof(Field2Static))]
        public void ConstructorShouldInitializeStaticMember(string fieldName)
        {
            var fieldInfo = GetType().GetField(fieldName)!;
            fieldInfo.ShouldNotBeNull();
            var reflectedType = typeof(object);
            var name = fieldName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = new ActionToken((o, o1) => { });
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

            memberInfo = new FieldAccessorMemberInfo(name, fieldInfo, reflectedType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(fieldInfo.FieldType);
            memberInfo.DeclaringType.ShouldEqual(fieldInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(fieldInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Accessor);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Static | MemberFlags.Public);

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
            observerRequestCount.ShouldEqual(1);

            if (fieldName == nameof(Field1Static))
            {
                Field1Static = "test";
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Field1Static);
                memberInfo.SetValue(this, nameof(Field1Static), DefaultMetadata);
                Field1Static.ShouldEqual(nameof(Field1Static));
            }
            else
            {
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(Field2Static);
                memberInfo.SetValue(this, int.MaxValue, DefaultMetadata);
                Field2Static.ShouldEqual(int.MaxValue);
            }
        }
    }
}