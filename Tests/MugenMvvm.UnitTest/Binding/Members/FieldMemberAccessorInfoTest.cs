using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTest.Binding.Observers;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class FieldMemberAccessorInfoTest : UnitTestBase
    {
        #region Fields

        public string Field1;
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
            var testEventListener = new TestEventListener();
            var result = new ActionToken((o, o1) => { });
            var count = 0;

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
            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg3, arg4) =>
                {
                    ++observerRequestCount;
                    o.ShouldEqual(fieldInfo);
                    arg3.ShouldEqual(typeof(FieldInfo));
                    arg4.ShouldEqual(DefaultMetadata);
                    type.ShouldEqual(reflectedType);
                    return memberObserver;
                }
            });

            var delegateProvider = new ReflectionDelegateProvider();
            delegateProvider.AddComponent(new ExpressionReflectionDelegateProviderComponent());

            var memberInfo = new FieldMemberAccessorInfo(name, fieldInfo, reflectedType, observerProvider, delegateProvider);
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