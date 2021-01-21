using System;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class MethodAccessorMemberInfoTest : UnitTestBase
    {
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
            var inputArgs = isLastParameterMetadata ? new object?[] {"1", 2, null} : new object[] {"1", 2};
            var checkGetterArgs = inputArgs.ToArray();
            if (isLastParameterMetadata)
                checkGetterArgs[checkGetterArgs.Length - 1] = DefaultMetadata;
            var checkSetterArgs = inputArgs.Concat(new object[] {setValue}).ToArray();
            if (isLastParameterMetadata)
                checkSetterArgs[checkSetterArgs.Length - 2] = DefaultMetadata;

            var result = new ActionToken((o, o1) => { });
            var testEventListener = new TestWeakEventListener();
            var observeCount = 0;
            var memberObserver = new MemberObserver((target, member, listener, meta) =>
            {
                ++observeCount;
                target.ShouldEqual(this);
                listener.ShouldEqual(testEventListener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, this);

            var memberFlags = MemberFlags.All;
            IMethodMemberInfo? getMethod = null;
            IMethodMemberInfo? setMethod = null;
            if (canRead)
            {
                getMethod = new TestMethodMemberInfo
                {
                    DeclaringType = declaringType,
                    MemberFlags = memberFlags,
                    Invoke = (target, args, metadata) =>
                    {
                        ++getValueCount;
                        target.ShouldEqual(this);
                        checkGetterArgs.ShouldEqual(args.AsList());
                        metadata.ShouldEqual(DefaultMetadata);
                        return getValue;
                    },
                    TryObserve = (o, listener, arg3) =>
                    {
                        ++observeCount;
                        o.ShouldEqual(this);
                        listener.ShouldEqual(testEventListener);
                        arg3.ShouldEqual(DefaultMetadata);
                        return result;
                    },
                    Type = type
                };
            }

            if (canWrite)
            {
                setMethod = new TestMethodMemberInfo
                {
                    DeclaringType = declaringType,
                    MemberFlags = memberFlags,
                    Invoke = (target, args, metadata) =>
                    {
                        ++setValueCount;
                        target.ShouldEqual(this);
                        checkSetterArgs.ShouldEqual(args.AsList());
                        metadata.ShouldEqual(DefaultMetadata);
                        return null;
                    },
                    GetParameters = () => new[] {new TestParameterInfo {ParameterType = type}}
                };
            }

            MethodAccessorMemberInfo? memberInfo = null;
            var reflectedType = typeof(string);
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

            memberInfo = new MethodAccessorMemberInfo(name, getMethod, setMethod, inputArgs, isLastParameterMetadata ? ArgumentFlags.Metadata : (EnumFlags<ArgumentFlags>) default,
                reflectedType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.DeclaringType.ShouldEqual(declaringType);
            memberInfo.Type.ShouldEqual(type);
            memberInfo.UnderlyingMember.ShouldBeNull();
            memberInfo.MemberFlags.ShouldEqual(memberFlags);
            memberInfo.CanWrite.ShouldEqual(canWrite);
            memberInfo.CanRead.ShouldEqual(canRead);
            memberInfo.GetArgs().ShouldEqual(inputArgs);

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            observeCount.ShouldEqual(1);
            observerRequestCount.ShouldEqual(!canRead && canWrite ? 1 : 0);

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
    }
}