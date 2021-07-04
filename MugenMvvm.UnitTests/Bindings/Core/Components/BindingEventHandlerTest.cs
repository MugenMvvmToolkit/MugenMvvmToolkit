using System;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.UnitTests.Commands.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    [Collection(SharedContext)]
    public class BindingEventHandlerTest : UnitTestBase
    {
        public BindingEventHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(MemberManager));
            RegisterDisposeToken(WithGlobalService(BindingManager));
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void OnAttachingShouldSubscribeToTargetEvent()
        {
            var target = new object();
            IMemberInfo member = new TestAccessorMemberInfo();
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    metadata.ShouldEqual(DefaultMetadata);
                    return new MemberPathLastMember(target, member);
                }
            };
            IAttachableComponent component = BindingEventHandler.Get(default, false, true);
            component.OnAttaching(Binding, DefaultMetadata).ShouldBeFalse();

            member = new TestEventInfo();
            component.OnAttaching(Binding, DefaultMetadata).ShouldBeFalse();

            IEventListener? eventListener = null;
            member = new TestEventInfo
            {
                TryObserve = (t, listener, m) =>
                {
                    t.ShouldEqual(target);
                    eventListener = listener;
                    m.ShouldEqual(DefaultMetadata);
                    return ActionToken.FromDelegate((o, o1) => eventListener = null);
                }
            };
            component.OnAttaching(Binding, DefaultMetadata).ShouldBeTrue();
            eventListener.ShouldNotBeNull();
        }

        [Fact]
        public void OnDetachedShouldClearValues()
        {
            var cmdParameter = new object();
            var enabledMember = new TestAccessorMemberInfo
            {
                CanWrite = true,
                SetValue = (o, o1, arg3) => { }
            };

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, t, m, f, r, meta) => enabledMember
            });

            IEventListener? listener = null;
            var component = BindingEventHandler.Get(new BindingParameterValue(cmdParameter, null), true, true);
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    return new MemberPathLastMember(this, new TestEventInfo
                    {
                        TryObserve = (o, l, arg3) =>
                        {
                            listener = l;
                            return ActionToken.FromDelegate((o1, o2) => listener = null);
                        }
                    });
                }
            };

            EventHandler? canExecuteHandler = null;
            var value = new TestCommand
            {
                AddCanExecuteChanged = handler => { canExecuteHandler = handler; },
                RemoveCanExecuteChanged = handler => { canExecuteHandler = null; }
            };

            ((IAttachableComponent)component).OnAttaching(Binding, DefaultMetadata).ShouldBeTrue();
            component.TrySetTargetValue(Binding, new MemberPathLastMember(this, new TestAccessorMemberInfo()), value, DefaultMetadata).ShouldBeTrue();
            listener.ShouldNotBeNull();
            canExecuteHandler.ShouldNotBeNull();
            component.CommandParameter.IsEmpty.ShouldBeFalse();

            ((IDetachableComponent)component).OnDetached(Binding, DefaultMetadata);
            listener.ShouldBeNull();
            canExecuteHandler.ShouldBeNull();
            component.CommandParameter.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ShouldHandleOneWayMode()
        {
            var invokeCount = 0;
            Binding.UpdateSource = () => throw new NotSupportedException();
            Binding.UpdateTarget = () => ++invokeCount;
            var eventHandlerBindingComponent = (BindingEventHandler.OneWay)BindingEventHandler.Get(default, true, false);
            eventHandlerBindingComponent.OnSourceLastMemberChanged(Binding, null!, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            eventHandlerBindingComponent.OnSourcePathMembersChanged(Binding, null!, DefaultMetadata);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void TryHandleShouldBeHandledByCommand()
        {
            IEventListener? eventListener = null;
            var sender = new object();
            var message = new object();
            var member = new TestEventInfo
            {
                TryObserve = (t, listener, m) =>
                {
                    eventListener = listener;
                    return ActionToken.FromDelegate((o, o1) => eventListener = null);
                }
            };
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, member)
            };

            var cmdParameter = new object();
            var component = BindingEventHandler.Get(new BindingParameterValue(cmdParameter, null), false, true);
            var executeCount = 0;
            var value = new TestCommand
            {
                Execute = o =>
                {
                    ++executeCount;
                    component.EventArgs.ShouldEqual(message);
                    o.ShouldEqual(cmdParameter);
                }
            };

            ((IAttachableComponent)component).OnAttaching(Binding, DefaultMetadata).ShouldBeTrue();
            component.TrySetTargetValue(Binding, default, value, DefaultMetadata);
            eventListener!.TryHandle(sender, message, DefaultMetadata).ShouldBeTrue();
            executeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryHandleShouldBeHandledByValueExpression()
        {
            IEventListener? eventListener = null;
            var sender = new object();
            var message = new object();
            var member = new TestEventInfo
            {
                TryObserve = (t, listener, m) =>
                {
                    eventListener = listener;
                    return ActionToken.FromDelegate((o, o1) => eventListener = null);
                }
            };
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, member)
            };

            var component = BindingEventHandler.Get(default, false, true);
            var executeCount = 0;
            var value = new TestValueExpression
            {
                Invoke = m =>
                {
                    ++executeCount;
                    m.ShouldEqual(DefaultMetadata);
                    component.EventArgs.ShouldEqual(message);
                    return null;
                }
            };

            ((IAttachableComponent)component).OnAttaching(Binding, DefaultMetadata).ShouldBeTrue();
            component.TrySetTargetValue(Binding, default, value, DefaultMetadata);
            eventListener!.TryHandle(sender, message, DefaultMetadata).ShouldBeTrue();
            executeCount.ShouldEqual(1);
        }

        [Fact]
        public void TrySetTargetValueShouldInitializeCommand()
        {
            var target = "";
            var cmdParameter = new object();

            var enabledValue = false;
            var canExecute = true;
            var enabledMember = new TestAccessorMemberInfo
            {
                CanWrite = true,
                SetValue = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    arg3.ShouldEqual(DefaultMetadata);
                    enabledValue = (bool)o1!;
                }
            };
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, t, m, f, r, meta) =>
                {
                    meta.ShouldEqual(DefaultMetadata);
                    r.ShouldEqual(BindableMembers.For<object>().Enabled().Name);
                    f.ShouldEqual(MemberFlags.All & ~(MemberFlags.NonPublic | MemberFlags.Static));
                    m.ShouldEqual(MemberType.Accessor);
                    t.ShouldEqual(target.GetType());
                    return enabledMember;
                }
            });

            var component = BindingEventHandler.Get(new BindingParameterValue(cmdParameter, null), true, true);
            EventHandler? canExecuteHandler = null;
            var value = new TestCommand
            {
                AddCanExecuteChanged = handler =>
                {
                    canExecuteHandler.ShouldBeNull();
                    canExecuteHandler = handler;
                },
                RemoveCanExecuteChanged = handler =>
                {
                    canExecuteHandler.ShouldNotBeNull();
                    canExecuteHandler = null;
                },
                CanExecute = o =>
                {
                    o.ShouldEqual(cmdParameter);
                    return canExecute;
                }
            };

            enabledValue.ShouldNotEqual(canExecute);
            component.TrySetTargetValue(Binding, new MemberPathLastMember(target, new TestAccessorMemberInfo()), value, DefaultMetadata).ShouldBeTrue();
            enabledValue.ShouldEqual(canExecute);
            canExecuteHandler.ShouldNotBeNull();

            canExecute = false;
            canExecuteHandler!.Invoke(this, EventArgs.Empty);
            enabledValue.ShouldEqual(canExecute);

            component.TrySetTargetValue(Binding, default, new TestValueExpression(), DefaultMetadata).ShouldBeTrue();
            enabledValue.ShouldBeTrue();
            canExecuteHandler.ShouldBeNull();
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetShouldInitializeValues(bool isOneTime)
        {
            var parameter = new BindingParameterValue(this, null);
            var eventHandlerBindingComponent = BindingEventHandler.Get(parameter, true, isOneTime);
            eventHandlerBindingComponent.ToggleEnabledState.ShouldBeTrue();
            eventHandlerBindingComponent.CommandParameter.ShouldEqual(parameter);
            if (isOneTime)
                eventHandlerBindingComponent.ShouldBeType<BindingEventHandler>();
            else
                eventHandlerBindingComponent.ShouldBeType<BindingEventHandler.OneWay>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnAttachedShouldUpdateTargetAddOneTimeModeIfNeed(bool addOneTimeMode)
        {
            var updateCount = 0;
            Binding.UpdateTarget = () => ++updateCount;
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
            };
            if (addOneTimeMode)
                Binding.Source = ItemOrArray.FromItem<object?>(new TestMemberPathObserver { GetLastMember = metadata => default });
            IAttachableComponent component = BindingEventHandler.Get(default, false, true);
            component.OnAttached(Binding, DefaultMetadata);
            updateCount.ShouldEqual(1);
            if (addOneTimeMode)
            {
                Binding.Components.Count.ShouldEqual(1);
                Binding.Components.Get<object>().Single().ShouldEqual(OneTimeBindingMode.NonDisposeInstance);
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void TryHandleShouldBeHandledByComponents(int count, bool isError)
        {
            IEventListener? eventListener = null;
            var sender = new object();
            var message = new object();
            var member = new TestEventInfo
            {
                TryObserve = (t, listener, m) =>
                {
                    eventListener = listener;
                    return ActionToken.FromDelegate((o, o1) => eventListener = null);
                }
            };
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, member)
            };

            var exception = new Exception();
            var beginEventCount = 0;
            var errorEventCount = 0;
            var endEventCount = 0;

            for (var i = 0; i < count; i++)
            {
                BindingManager.AddComponent(new TestBindingEventHandlerComponent
                {
                    OnBeginEvent = (_, s, msg, metadata) =>
                    {
                        if (isError)
                            throw exception;
                        ++beginEventCount;
                        s.ShouldEqual(sender);
                        msg.ShouldEqual(message);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    OnEndEvent = (_, s, msg, metadata) =>
                    {
                        ++endEventCount;
                        s.ShouldEqual(sender);
                        msg.ShouldEqual(message);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    OnEventError = (_, e, s, msg, metadata) =>
                    {
                        ++errorEventCount;
                        e.ShouldEqual(exception);
                        s.ShouldEqual(sender);
                        msg.ShouldEqual(message);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            var component = BindingEventHandler.Get(default, false, true);
            ((IAttachableComponent)component).OnAttaching(Binding, DefaultMetadata).ShouldBeTrue();

            component.TrySetTargetValue(Binding, default, new TestValueExpression(), DefaultMetadata);
            eventListener!.TryHandle(sender, message, DefaultMetadata).ShouldBeTrue();
            if (isError)
                errorEventCount.ShouldEqual(count);
            else
                beginEventCount.ShouldEqual(count);
            endEventCount.ShouldEqual(count);
        }
    }
}