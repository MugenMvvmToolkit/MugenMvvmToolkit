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
    public class FieldAccessorMemberInfoTest : UnitTestBase
    {
        #region Fields

        public string? Field1;
        public int Field2;

        #endregion

        #region Methods

        [Theory]
        [InlineData(nameof(Field1))]
        [InlineData(nameof(Field2))]
        public void ConstructorShouldInitializeMember(string fieldName)
        {
            var fieldInfo = GetType().GetField(fieldName);
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
            var observerProvider = new ObservationManager();
            observerProvider.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg3, arg4) =>
                {
                    ++observerRequestCount;
                    o.ShouldEqual(memberInfo);
                    arg3.ShouldEqual(typeof(FieldAccessorMemberInfo));
                    arg4.ShouldEqual(DefaultMetadata);
                    type.ShouldEqual(reflectedType);
                    return memberObserver;
                }
            });

            var delegateProvider = new ReflectionManager();
            delegateProvider.AddComponent(new ExpressionReflectionDelegateProvider());

            memberInfo = new FieldAccessorMemberInfo(name, fieldInfo, reflectedType, observerProvider, delegateProvider);
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

        #endregion
    }
}