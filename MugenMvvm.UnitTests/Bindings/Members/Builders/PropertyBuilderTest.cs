using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Builders
{
    [Collection(SharedContext)]
    public class PropertyBuilderTest : UnitTestBase
    {
        public PropertyBuilderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(AttachedValueManager));
            RegisterDisposeToken(WithGlobalService(ObservationManager));
            RegisterDisposeToken(WithGlobalService(MemberManager));
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void ConstructorShouldInitializeValues(bool isStatic, bool nonObservable)
        {
            string name = "t";
            Type declaringType = typeof(object);
            var propertyType = typeof(Action);
            var member = new object();
            var builder = new PropertyBuilder<object, object>(name, declaringType, propertyType).UnderlyingMember(member);
            if (isStatic)
                builder = builder.Static();
            if (nonObservable)
                builder = builder.NonObservable();
            var build = builder.Build();
            build.MemberType.ShouldEqual(MemberType.Accessor);
            build.Type.ShouldEqual(propertyType);
            build.DeclaringType.ShouldEqual(declaringType);
            build.MemberFlags.ShouldEqual((isStatic ? MemberFlags.Attached | MemberFlags.StaticPublic : MemberFlags.Attached | MemberFlags.InstancePublic) |
                                          (nonObservable ? MemberFlags.NonObservable : default));
            build.UnderlyingMember.ShouldEqual(member);
            build.Name.ShouldEqual(name);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CustomObservableAutoHandlerShouldRaise(bool withAttachedHandler, bool isStatic)
        {
            var message = "m";
            var target = isStatic ? null : new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).CustomGetter((member, o, metadata) => "").ObservableAutoHandler();
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(Metadata);
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
                    metadata.ShouldEqual(Metadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(target, testEventHandler, Metadata);
            ((INotifiableMemberInfo)memberInfo).Raise(target, message, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            ((INotifiableMemberInfo)memberInfo).Raise(target, message, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
            {
                memberInfo.TryObserve(target, testEventHandler, Metadata);
                attachedInvokeCount.ShouldEqual(1);
            }
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CustomShouldUseDelegates(bool withAttachedHandler, bool isStatic)
        {
            var message = "m";
            var target = new object();
            var testEventHandler = new TestWeakEventListener();
            var attachedInvokeCount = 0;
            var invokeCount = 0;
            var raiseInvokeCount = 0;
            var result = ActionToken.FromDelegate((o, o1) => { });
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).CustomGetter((member, o, metadata) => "").ObservableHandler(
                (member, o, listener, metadata) =>
                {
                    ++invokeCount;
                    member.ShouldEqual(memberInfo);
                    o.ShouldEqual(target);
                    listener.ShouldEqual(testEventHandler);
                    metadata.ShouldEqual(Metadata);
                    return result;
                }, (member, o, msg, metadata) =>
                {
                    ++raiseInvokeCount;
                    member.ShouldEqual(memberInfo);
                    o.ShouldEqual(target);
                    msg.ShouldEqual(message);
                    metadata.ShouldEqual(Metadata);
                });
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(Metadata);
                });
            }

            if (isStatic)
                builder = builder.Static();
            memberInfo = builder.Build();
            memberInfo.TryObserve(target, testEventHandler, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
            raiseInvokeCount.ShouldEqual(0);
            ((INotifiableMemberInfo)memberInfo).Raise(target, message, Metadata);
            invokeCount.ShouldEqual(1);
            raiseInvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            if (withAttachedHandler)
            {
                memberInfo.TryObserve(target, testEventHandler, Metadata);
                attachedInvokeCount.ShouldEqual(1);
            }
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void DefaultValueShouldSetDefaultValue1(bool isStatic, bool inherits)
        {
            object defaultValue = this;
            var builder = new PropertyBuilder<object, object>(nameof(DefaultValueShouldSetDefaultValue1), typeof(object), typeof(EventHandler));
            builder.DefaultValue(defaultValue);
            if (isStatic)
                builder.Static();
            if (inherits)
                builder.Inherits();

            var memberInfo = builder.Build();
            memberInfo.GetValue(isStatic ? null : this, Metadata).ShouldEqual(defaultValue);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void DefaultValueShouldSetDefaultValue2(bool isStatic, bool inherits)
        {
            var invokeCount = 0;
            var target = isStatic ? null : new object();
            object defaultValue = this;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>(nameof(DefaultValueShouldSetDefaultValue2), typeof(object), typeof(EventHandler));
            builder.DefaultValue((m, o) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                return defaultValue;
            });
            if (isStatic)
                builder.Static();
            if (inherits)
                builder.Inherits();

            memberInfo = builder.Build();
            memberInfo.GetValue(target, Metadata).ShouldEqual(defaultValue);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void GetSetAutoGeneratedInstanceShouldUseCorrectValues(int count, bool inherits)
        {
            var targets = new List<object>();
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object));
            if (inherits)
                builder = builder.Inherits();
            var member = builder.Build();
            for (var i = 0; i < count; i++)
            {
                var t = new object();
                targets.Add(t);
                member.GetValue(t).ShouldEqual(null);
                member.SetValue(t, i);
            }

            for (var i = 0; i < count; i++)
                member.GetValue(targets[i]).ShouldEqual(i);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetSetAutoGeneratedShouldCallAttachedHandler(bool inherits, bool isStatic)
        {
            var target = isStatic ? null : new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object)).AttachedHandler((member, t, metadata) =>
            {
                ++attachedInvokeCount;
                member.ShouldEqual(memberInfo);
                t.ShouldEqual(target);
                metadata.ShouldEqual(Metadata);
            });
            if (inherits)
                builder.Inherits();
            if (isStatic)
                builder.Static();
            memberInfo = builder.Build();
            memberInfo.GetValue(target, Metadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.GetValue(target, Metadata);
            memberInfo.SetValue(target, this, Metadata);
            memberInfo.SetValue(target, this, Metadata);
            attachedInvokeCount.ShouldEqual(1);

            if (isStatic)
                return;
            attachedInvokeCount = 0;
            target = new object();
            memberInfo.SetValue(target, this, Metadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.SetValue(target, this, Metadata);
            memberInfo.GetValue(target, Metadata);
            memberInfo.GetValue(target, Metadata);
            attachedInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void GetSetAutoGeneratedStaticShouldUseCorrectValues()
        {
            var builder = new PropertyBuilder<object, object>(NewId(), typeof(object), typeof(object)).Static();
            var member = builder.Build();
            member.GetValue(null).ShouldBeNull();
            member.SetValue(null, this);
            member.GetValue(null).ShouldEqual(this);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetSetCustomShouldCallAttachedHandler(bool isStatic)
        {
            var target = isStatic ? null : new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>(NewId(), typeof(object), typeof(object))
                          .CustomGetter((member, o, metadata) => "")
                          .CustomSetter((member, o, value, metadata) => { })
                          .AttachedHandler((member, t, metadata) =>
                          {
                              ++attachedInvokeCount;
                              member.ShouldEqual(memberInfo);
                              t.ShouldEqual(target);
                              metadata.ShouldEqual(Metadata);
                          });

            if (isStatic)
                builder = builder.Static();

            memberInfo = builder.Build();
            memberInfo.GetValue(target, Metadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.GetValue(target, Metadata);
            memberInfo.SetValue(target, this, Metadata);
            memberInfo.SetValue(target, this, Metadata);
            attachedInvokeCount.ShouldEqual(1);

            if (isStatic)
                return;
            attachedInvokeCount = 0;
            target = new object();
            memberInfo.SetValue(target, this, Metadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.SetValue(target, this, Metadata);
            memberInfo.GetValue(target, Metadata);
            memberInfo.GetValue(target, Metadata);
            attachedInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void GetSetCustomShouldUseDelegates()
        {
            var target = new object();
            var resultValue = new object();
            var setValue = new object();
            var getCount = 0;
            var setCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object))
                          .CustomGetter((member, t, metadata) =>
                          {
                              ++getCount;
                              member.ShouldEqual(memberInfo);
                              t.ShouldEqual(target);
                              metadata.ShouldEqual(Metadata);
                              return resultValue;
                          })
                          .CustomSetter((member, t, value, metadata) =>
                          {
                              ++setCount;
                              member.ShouldEqual(memberInfo);
                              t.ShouldEqual(target);
                              value.ShouldEqual(setValue);
                              metadata.ShouldEqual(Metadata);
                          });

            memberInfo = builder.Build();
            memberInfo.GetValue(target, Metadata);
            getCount.ShouldEqual(1);
            setCount.ShouldEqual(0);
            memberInfo.SetValue(target, setValue, Metadata);
            getCount.ShouldEqual(1);
            setCount.ShouldEqual(1);
        }

        [Fact]
        public void InheritsShouldUseParentValue()
        {
            var parent = new object();
            var target = new object();
            var canReturnParent = false;
            IEventListener? parentListener = null;
            var parentMember = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    if (!canReturnParent)
                        return null;
                    if (o == target)
                        return parent;
                    return null;
                },
                TryObserve = (o, listener, arg3) =>
                {
                    if (o == target)
                    {
                        if (parentListener != null)
                            throw new NotSupportedException();
                        parentListener = listener;
                        return ActionToken.FromDelegate((o1, o2) => parentListener = null);
                    }

                    return default;
                }
            };
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, _, _, _, arg4, _) =>
                {
                    if (BindableMembers.For<object>().Parent().Name.Equals(arg4))
                        return ItemOrIReadOnlyList.FromItem<IMemberInfo>(parentMember);
                    return default;
                }
            });

            var changedCount = 0;
            var parentValue = new object();
            object defaultValue = new();
            var memberInfo = new PropertyBuilder<object, object>("t", typeof(object), typeof(object)).Inherits().DefaultValue(defaultValue).Build();
            memberInfo.GetValue(target).ShouldEqual(defaultValue);
            memberInfo.TryObserve(target, new TestWeakEventListener
            {
                TryHandle = (o, o1, arg3) =>
                {
                    ++changedCount;
                    return true;
                }
            });

            memberInfo.SetValue(parent, parentValue);
            canReturnParent = true;
            parentListener?.TryHandle(parent, parent, Metadata);
            changedCount.ShouldEqual(1);
            memberInfo.GetValue(target).ShouldEqual(parentValue);

            memberInfo.SetValue(parent, null);
            changedCount.ShouldEqual(2);
            memberInfo.GetValue(target).ShouldEqual(null);

            canReturnParent = false;
            parentListener?.TryHandle(parent, parent, Metadata);
            changedCount.ShouldEqual(3);
            memberInfo.GetValue(target).ShouldEqual(defaultValue);

            canReturnParent = true;
            parentListener?.TryHandle(parent, parent, Metadata);
            changedCount.ShouldEqual(4);
            memberInfo.GetValue(target).ShouldEqual(null);

            memberInfo.SetValue(target, this);
            changedCount.ShouldEqual(5);
            parentListener.ShouldBeNull();
            memberInfo.GetValue(target).ShouldEqual(this);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void InstanceAutoGeneratedShouldRaiseOnChange(bool withAttachedHandler, bool inherits, bool observable)
        {
            var message = "m";
            var target = new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            object? oldV = null;
            object? newV = this;
            var propertyChangedInvokeCount = 0;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).PropertyChangedHandler((member, t, value, newValue, metadata) =>
            {
                ++propertyChangedInvokeCount;
                memberInfo.ShouldEqual(member);
                t.ShouldEqual(target);
                value.ShouldEqual(oldV);
                newValue.ShouldEqual(newV);
                metadata.ShouldEqual(Metadata);
            });
            if (!observable)
                builder = builder.NonObservable();
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(Metadata);
                });
            }

            if (inherits)
                builder = builder.Inherits();

            memberInfo = builder.Build();
            var testEventHandler = new TestWeakEventListener
            {
                TryHandle = (t, msg, metadata) =>
                {
                    t.ShouldEqual(target);
                    message.ShouldNotBeNull();
                    metadata.ShouldEqual(Metadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(target, testEventHandler, Metadata);
            memberInfo.SetValue(target, newV, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(observable ? 1 : 0);
            propertyChangedInvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            memberInfo.SetValue(target, newV, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(observable ? 1 : 0);
            propertyChangedInvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            oldV = this;
            newV = null;
            memberInfo.SetValue(target, newV, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(observable ? 1 : 0);
            propertyChangedInvokeCount.ShouldEqual(2);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldUseObservationManagerAutoProperty()
        {
            var invokeCount = 0;
            var memberInfo = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).NonObservable().Build();
            memberInfo.MemberFlags.HasFlag(MemberFlags.NonObservable).ShouldBeTrue();
            ObservationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (_, type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(memberInfo.DeclaringType);
                    o.ShouldEqual(memberInfo);
                    arg3.ShouldEqual(Metadata);
                    return MemberObserver.NoDo;
                }
            });

            memberInfo.TryObserve(this, new TestWeakEventListener(), Metadata);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldUseObservationManagerCustomProperty(bool observable)
        {
            var invokeCount = 0;
            var actionToken = ActionToken.FromDelegate((o, o1) => { });
            var b = new CustomPropertyBuilder<object, object>(new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler))).CustomGetter((member, target,
                metadata) => "");
            if (!observable)
                b.NonObservable();
            var memberInfo = b.Build();
            memberInfo.MemberFlags.HasFlag(MemberFlags.NonObservable).ShouldEqual(!observable);
            ObservationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (_, type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(memberInfo.DeclaringType);
                    o.ShouldEqual(memberInfo);
                    arg3.ShouldEqual(Metadata);
                    return new MemberObserver((o1, o2, listener, arg4) => actionToken, memberInfo);
                }
            });

            var token = memberInfo.TryObserve(this, new TestWeakEventListener(), Metadata);
            invokeCount.ShouldEqual(1);
            token.ShouldEqual(actionToken);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void StaticAutoGeneratedShouldRaiseOnChange(bool withAttachedHandler, bool observable)
        {
            var message = "m";
            IAccessorMemberInfo? memberInfo = null;
            object? oldV = null;
            object? newV = this;
            var attachedInvokeCount = 0;
            var propertyChangedInvokeCount = 0;

            var builder = new PropertyBuilder<object, object>(NewId(), typeof(object), typeof(EventHandler))
                          .Static()
                          .PropertyChangedHandler((member, target, value, newValue, metadata) =>
                          {
                              ++propertyChangedInvokeCount;
                              memberInfo.ShouldEqual(member);
                              target.ShouldBeNull();
                              value.ShouldEqual(oldV);
                              newValue.ShouldEqual(newV);
                              metadata.ShouldEqual(Metadata);
                          });
            if (!observable)
                builder = builder.NonObservable();
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldBeNull();
                    metadata.ShouldEqual(Metadata);
                });
            }

            memberInfo = builder.Build();
            var testEventHandler = new TestWeakEventListener
            {
                TryHandle = (t, msg, meta) =>
                {
                    t.ShouldBeNull();
                    message.ShouldNotBeNull();
                    meta.ShouldEqual(Metadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(null, testEventHandler, Metadata);
            memberInfo.SetValue(null, newV, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(observable ? 1 : 0);
            propertyChangedInvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            memberInfo.SetValue(null, newV, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(observable ? 1 : 0);
            propertyChangedInvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            oldV = this;
            newV = null;
            memberInfo.SetValue(null, newV, Metadata);
            testEventHandler.InvokeCount.ShouldEqual(observable ? 1 : 0);
            propertyChangedInvokeCount.ShouldEqual(2);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WrapShouldUseWrappedMember(bool isStatic)
        {
            var target = isStatic ? null : "";
            var value = new object();
            var newValue = new object();

            var wrappedBuilder = new PropertyBuilder<object, object?>("auto", typeof(object), typeof(object));
            if (isStatic)
                wrappedBuilder = wrappedBuilder.Static();

            var wrappedMember = wrappedBuilder.Build();
            var invokeCount = 0;
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, type, memberType, arg3, arg4, arg6) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(isStatic ? typeof(object) : target!.GetType());
                    memberType.ShouldEqual(MemberType.Accessor);
                    arg3.ShouldEqual(MemberFlags.All.SetInstanceOrStaticFlags(isStatic));
                    wrappedMember.Name.ShouldEqual(arg4);
                    arg6.ShouldEqual(Metadata);
                    return ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(wrappedMember);
                }
            });
            var listener = new TestWeakEventListener
            {
                TryHandle = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    arg3.ShouldEqual(Metadata);
                    return true;
                }
            };
            var propertyBuilder = new PropertyBuilder<object, object>("wrap", typeof(object), typeof(object));
            if (isStatic)
                propertyBuilder = propertyBuilder.Static();
            var member = propertyBuilder.WrapMember(wrappedMember.Name).Build();
            member.GetValue(target, Metadata).ShouldBeNull();
            invokeCount.ShouldEqual(1);
            member.TryObserve(target, listener);

            listener.InvokeCount.ShouldEqual(0);
            wrappedMember.SetValue(target, value, Metadata);
            listener.InvokeCount.ShouldEqual(1);
            member.GetValue(target, Metadata).ShouldEqual(value);

            wrappedMember.SetValue(target, null, Metadata);
            listener.InvokeCount.ShouldEqual(2);

            member.SetValue(target, newValue, Metadata);
            listener.InvokeCount.ShouldEqual(3);

            wrappedMember.SetValue(target, value, Metadata);
            listener.InvokeCount.ShouldEqual(3);
        }

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);
    }
}