﻿using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Binding.Members.Internal;
using MugenMvvm.UnitTests.Binding.Observation.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Members.Builders
{
    public class MethodBuilderTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructorShouldInitializeValues(bool isStatic)
        {
            string name = "t";
            Type declaringType = typeof(object);
            var propertyType = typeof(Action);
            var member = new object();
            var builder = new MethodBuilder<object, object>(name, declaringType, propertyType)
                .UnderlyingMember(member)
                .InvokeHandler((info, target, args, metadata) => "");
            if (isStatic)
                builder = builder.Static();
            var build = builder.Build();
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Type.ShouldEqual(propertyType);
            build.DeclaringType.ShouldEqual(declaringType);
            build.AccessModifiers.ShouldEqual(isStatic ? MemberFlags.Attached | MemberFlags.StaticPublic : MemberFlags.Attached | MemberFlags.InstancePublic);
            build.UnderlyingMember.ShouldEqual(member);
            build.Name.ShouldEqual(name);
        }

        [Fact]
        public void WithParametersShouldInitializeParameters1()
        {
            var parameterInfos = new IParameterInfo[1] {new TestParameterInfo()};
            var memberInfo = new MethodBuilder<object, object>("t", typeof(object), typeof(EventHandler))
                .WithParameters(parameterInfos)
                .InvokeHandler((info, target, args, metadata) => "")
                .Build();
            memberInfo.GetParameters().ShouldEqual(parameterInfos);
        }

        [Fact]
        public void WithParametersShouldInitializeParameters2()
        {
            var invokeCount = 0;
            IMethodMemberInfo? method = null;
            var parameterInfos = new IParameterInfo[] {new TestParameterInfo()};
            method = new MethodBuilder<object, object>("t", typeof(object), typeof(EventHandler))
                .GetParametersHandler(info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(method);
                    return parameterInfos;
                })
                .InvokeHandler((info, target, args, metadata) => "")
                .Build();
            method.GetParameters().ShouldEqual(parameterInfos);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetAccessorHandlerShouldUseDelegate()
        {
            var flags = ArgumentFlags.Metadata;
            var values = new object[] {this};
            IMethodMemberInfo? memberInfo = null;

            var invokeCount = 0;
            var accessor = new TestAccessorMemberInfo();
            memberInfo = new MethodBuilder<object, object>("t", typeof(object), typeof(object))
                .TryGetAccessorHandler((member, argumentFlags, args, metadata) =>
                {
                    ++invokeCount;
                    member.ShouldEqual(memberInfo);
                    argumentFlags.ShouldEqual(flags);
                    args.ShouldEqual(values);
                    metadata.ShouldEqual(DefaultMetadata);
                    return accessor;
                })
                .InvokeHandler((member, target, args, metadata) => "")
                .Build();
            memberInfo.TryGetAccessor(flags, values, DefaultMetadata).ShouldEqual(accessor);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void ObservableShouldUseDelegates(bool withAttachedHandler, bool isStatic)
        {
            var message = "m";
            var target = isStatic ? null : new object();
            var testEventHandler = new TestWeakEventListener();
            var attachedInvokeCount = 0;
            var invokeCount = 0;
            var raiseInvokeCount = 0;
            var result = new ActionToken((o, o1) => { });
            IMethodMemberInfo? memberInfo = null;
            var builder = new MethodBuilder<object, object>("t", typeof(object), typeof(EventHandler)).InvokeHandler((member, o, args, metadata) => "").ObservableHandler((member, o, listener, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(memberInfo);
                o.ShouldEqual(target);
                listener.ShouldEqual(testEventHandler);
                metadata.ShouldEqual(DefaultMetadata);
                return result;
            }, (member, o, msg, metadata) =>
            {
                ++raiseInvokeCount;
                member.ShouldEqual(memberInfo);
                o.ShouldEqual(target);
                msg.ShouldEqual(message);
                metadata.ShouldEqual(DefaultMetadata);
            });
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                });
            }

            if (isStatic)
                builder = builder.Static();
            memberInfo = builder.Build();
            memberInfo.TryObserve(target, testEventHandler, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
            raiseInvokeCount.ShouldEqual(0);
            ((INotifiableMemberInfo) memberInfo).Raise(target, message, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            raiseInvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            if (withAttachedHandler)
            {
                memberInfo.TryObserve(target, testEventHandler, DefaultMetadata);
                attachedInvokeCount.ShouldEqual(1);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldUseObservationManager(bool observable)
        {
            var invokeCount = 0;
            var actionToken = new ActionToken((o, o1) => { });
            var b = new MethodBuilder<object, object>("t", typeof(object), typeof(EventHandler)).InvokeHandler((member, target, args, metadata) => "");
            if (!observable)
                b.NonObservable();
            var memberInfo = b.Build();
            using var _ = TestComponentSubscriber.Subscribe(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(memberInfo.DeclaringType);
                    o.ShouldEqual(memberInfo);
                    arg3.ShouldEqual(DefaultMetadata);
                    return new MemberObserver((o1, o2, listener, arg4) => actionToken, memberInfo);
                }
            });

            var token = memberInfo.TryObserve(this, new TestWeakEventListener(), DefaultMetadata);
            if (observable)
            {
                invokeCount.ShouldEqual(1);
                token.ShouldEqual(actionToken);
            }
            else
            {
                invokeCount.ShouldEqual(0);
                token.IsEmpty.ShouldBeTrue();
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void ObservableShouldRaise(bool withAttachedHandler, bool isStatic)
        {
            var message = "m";
            var target = isStatic ? null : new object();
            var attachedInvokeCount = 0;
            IMethodMemberInfo? memberInfo = null;
            var builder = new MethodBuilder<object, object>("t", typeof(object), typeof(EventHandler)).InvokeHandler((member, o, args, metadata) => "").Observable();
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                });
            }

            if (isStatic)
                builder.Static();
            memberInfo = builder.Build();
            var testEventHandler = new TestWeakEventListener
            {
                TryHandle = (t, msg, metadata) =>
                {
                    t.ShouldEqual(isStatic ? typeof(object) : target);
                    message.ShouldEqual(msg);
                    metadata.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(target, testEventHandler, DefaultMetadata);
            ((INotifiableMemberInfo) memberInfo).Raise(target, message, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            ((INotifiableMemberInfo) memberInfo).Raise(target, message, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
            {
                memberInfo.TryObserve(target, testEventHandler, DefaultMetadata);
                attachedInvokeCount.ShouldEqual(1);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InvokeShouldCallAttachedHandler(bool isStatic)
        {
            var target = isStatic ? null : new object();
            var attachedInvokeCount = 0;
            IMethodMemberInfo? memberInfo = null;
            var builder = new MethodBuilder<object, object>("t", typeof(object), typeof(EventHandler)).InvokeHandler((member, o, args, metadata) => "")
                .AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                });

            if (isStatic)
                builder.Static();

            memberInfo = builder.Build();
            memberInfo.Invoke(target, Default.Array<object>(), DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.Invoke(target, Default.Array<object>(), DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void InvokeShouldUseDelegates(bool withAttachedHandler, bool isStatic)
        {
            var target = isStatic ? null : new object();
            var resultValue = new object();
            var parameters = new object[] {""};
            var invokeCount = 0;
            IMethodMemberInfo? memberInfo = null;
            var builder = new MethodBuilder<object, object>("t", typeof(object), typeof(EventHandler))
                .InvokeHandler((member, t, args, metadata) =>
                {
                    ++invokeCount;
                    member.ShouldEqual(memberInfo);
                    args.ShouldEqual(parameters);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                    return resultValue;
                });
            if (withAttachedHandler)
                builder = builder.AttachedHandler((member, o, metadata) => { });
            if (isStatic)
                builder = builder.Static();

            memberInfo = builder.Build();
            memberInfo.Invoke(target, parameters, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}