using System;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class MethodAccessorMemberInfoTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        public void ConstructorShouldInitializeMember(bool canWrite, bool canRead, bool isLastParameterMetadata)
        {
            var type = typeof(long);
            string name = "m";
            var getValue = "1";
            var setValue = "2";
            var getValueCount = 0;
            var setValueCount = 0;
            var declaringType = typeof(object);
            var inputArgs = isLastParameterMetadata ? new object?[] { "1", 2, null } : new object[] { "1", 2 };
            var checkGetterArgs = inputArgs.ToArray();
            if (isLastParameterMetadata)
                checkGetterArgs[checkGetterArgs.Length - 1] = DefaultMetadata;
            var checkSetterArgs = inputArgs.Concat(new object[] { setValue }).ToArray();
            if (isLastParameterMetadata)
                checkSetterArgs[checkSetterArgs.Length - 2] = DefaultMetadata;

            var memberFlags = MemberFlags.All;
            IMethodInfo? getMethod = null;
            IMethodInfo? setMethod = null;
            if (canRead)
            {
                getMethod = new TestMethodInfo
                {
                    DeclaringType = declaringType,
                    AccessModifiers = memberFlags,
                    Invoke = (target, args, metadata) =>
                    {
                        ++getValueCount;
                        target.ShouldEqual(this);
                        checkGetterArgs.ShouldEqual(args);
                        metadata.ShouldEqual(DefaultMetadata);
                        return getValue;
                    },
                    Type = type
                };
            }

            if (canWrite)
            {
                setMethod = new TestMethodInfo
                {
                    DeclaringType = declaringType,
                    AccessModifiers = memberFlags,
                    Invoke = (target, args, metadata) =>
                    {
                        ++setValueCount;
                        target.ShouldEqual(this);
                        checkSetterArgs.ShouldEqual(args);
                        metadata.ShouldEqual(DefaultMetadata);
                        return null;
                    },
                    GetParameters = () => new[] { new TestParameterInfo { ParameterType = type } }
                };
            }

            MethodAccessorMemberInfo? memberInfo = null;
            var reflectedType = typeof(string);
            var testEventListener = new TestWeakEventListener();
            var result = new ActionToken((o, o1) => { });
            var count = 0;
            var memberObserver = new MemberObserver((target, member, listener, meta) =>
            {
                ++count;
                target.ShouldEqual(this);
                listener.ShouldEqual(testEventListener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, this);

            var observerRequestCount = 0;
            var observerProvider = new ObserverProvider();
            observerProvider.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg3, arg4) =>
                {
                    ++observerRequestCount;
                    o.ShouldEqual(memberInfo);
                    arg3.ShouldEqual(typeof(MethodAccessorMemberInfo));
                    arg4.ShouldEqual(DefaultMetadata);
                    type.ShouldEqual(reflectedType);
                    return memberObserver;
                }
            });

            memberInfo = new MethodAccessorMemberInfo(name, getMethod, setMethod, inputArgs, isLastParameterMetadata ? ArgumentFlags.Metadata : 0, reflectedType, observerProvider);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.DeclaringType.ShouldEqual(declaringType);
            memberInfo.Type.ShouldEqual(type);
            memberInfo.UnderlyingMember.ShouldBeNull();
            memberInfo.AccessModifiers.ShouldEqual(memberFlags);
            memberInfo.CanWrite.ShouldEqual(canWrite);
            memberInfo.CanRead.ShouldEqual(canRead);

            memberInfo.GetArgs().ShouldEqual(inputArgs);

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
            observerRequestCount.ShouldEqual(1);

            if (canRead)
            {
                memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(getValue);
                getValueCount.ShouldEqual(1);
            }
            else
                ShouldThrow<InvalidOperationException>(() => memberInfo.GetValue(this, DefaultMetadata));

            if (canWrite)
            {
                memberInfo.SetValue(this, setValue, DefaultMetadata);
                setValueCount.ShouldEqual(1);
            }
            else
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, setValue, DefaultMetadata));
        }

        #endregion
    }
}