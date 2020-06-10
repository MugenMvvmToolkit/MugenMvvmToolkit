using System;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Commands.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components.Binding
{
    public class EventHandlerBindingComponentTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetShouldInitializeValues(bool isOneTime)
        {
            var parameter = new BindingParameterValue(this, null);
            var eventHandlerBindingComponent = EventHandlerBindingComponent.Get(parameter, true, isOneTime);
            eventHandlerBindingComponent.ToggleEnabledState.ShouldBeTrue();
            eventHandlerBindingComponent.CommandParameter.ShouldEqual(parameter);
            if (isOneTime)
                eventHandlerBindingComponent.ShouldBeType<EventHandlerBindingComponent>();
            else
                eventHandlerBindingComponent.ShouldBeType<EventHandlerBindingComponent.OneWay>();
        }

        [Fact]
        public void OnAttachingShouldSubscribeToTargetEvent()
        {
            var target = new object();
            IMemberInfo member = new TestAccessorMemberInfo();
            var binding = new TestBinding
            {
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata =>
                    {
                        metadata.ShouldEqual(DefaultMetadata);
                        return new MemberPathLastMember(target, member);
                    }
                }
            };
            IAttachableComponent component = EventHandlerBindingComponent.Get(default, false, true);
            component.OnAttaching(binding, DefaultMetadata).ShouldBeFalse();

            member = new TestEventInfo();
            component.OnAttaching(binding, DefaultMetadata).ShouldBeFalse();

            IEventListener? eventListener = null;
            member = new TestEventInfo
            {
                TryObserve = (t, listener, m) =>
                {
                    t.ShouldEqual(target);
                    eventListener = listener;
                    m.ShouldEqual(DefaultMetadata);
                    return new ActionToken((o, o1) => eventListener = null);
                }
            };
            component.OnAttaching(binding, DefaultMetadata).ShouldBeTrue();
            eventListener.ShouldNotBeNull();
        }

        [Fact]
        public void OnAttachedShouldUpdateTarget()
        {
            var updateCount = 0;
            var binding = new TestBinding
            {
                UpdateTarget = () => ++updateCount
            };
            IAttachableComponent component = EventHandlerBindingComponent.Get(default, false, true);
            component.OnAttached(binding, DefaultMetadata);
            updateCount.ShouldEqual(1);
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
                    return new ActionToken((o, o1) => eventListener = null);
                }
            };
            var binding = new TestBinding
            {
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, member)
                }
            };

            var exception = new Exception();
            var beginEventCount = 0;
            var errorEventCount = 0;
            var endEventCount = 0;
            var manager = new BindingManager();
            for (var i = 0; i < count; i++)
            {
                manager.AddComponent(new TestBindingEventHandlerComponent
                {
                    OnBeginEvent = (s, msg, metadata) =>
                    {
                        if (isError)
                            throw exception;
                        ++beginEventCount;
                        s.ShouldEqual(sender);
                        msg.ShouldEqual(message);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    OnEndEvent = (s, msg, metadata) =>
                    {
                        ++endEventCount;
                        s.ShouldEqual(sender);
                        msg.ShouldEqual(message);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    OnEventError = (e, s, msg, metadata) =>
                    {
                        ++errorEventCount;
                        e.ShouldEqual(exception);
                        s.ShouldEqual(sender);
                        msg.ShouldEqual(message);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            var component = EventHandlerBindingComponent.Get(default, false, true, manager);
            ((IAttachableComponent)component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();

            component.TrySetTargetValue(binding, default, new TestValueExpression(), DefaultMetadata);
            eventListener!.TryHandle(sender, message, DefaultMetadata).ShouldBeTrue();
            if (isError)
                errorEventCount.ShouldEqual(count);
            else
                beginEventCount.ShouldEqual(count);
            endEventCount.ShouldEqual(count);
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
                    return new ActionToken((o, o1) => eventListener = null);
                }
            };
            var binding = new TestBinding
            {
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, member)
                }
            };

            var cmdParameter = new object();
            var component = EventHandlerBindingComponent.Get(new BindingParameterValue(cmdParameter, null), false, true);
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

            ((IAttachableComponent)component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();
            component.TrySetTargetValue(binding, default, value, DefaultMetadata);
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
                    return new ActionToken((o, o1) => eventListener = null);
                }
            };
            var binding = new TestBinding
            {
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, member)
                }
            };

            var component = EventHandlerBindingComponent.Get(default, false, true);
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

            ((IAttachableComponent)component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();
            component.TrySetTargetValue(binding, default, value, DefaultMetadata);
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
            var manager = new BindingManager();
            var memberManager = new MemberManager();
            manager.AddComponent(memberManager);
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, tt, meta) =>
                {
                    meta.ShouldEqual(DefaultMetadata);
                    r.ShouldEqual(BindableMembers.Object.Enabled.ToString());
                    f.ShouldEqual(MemberFlags.All & ~(MemberFlags.NonPublic | MemberFlags.Static));
                    m.ShouldEqual(MemberType.Accessor);
                    t.ShouldEqual(target.GetType());
                    return enabledMember;
                }
            });

            var component = EventHandlerBindingComponent.Get(new BindingParameterValue(cmdParameter, null), true, true, manager);
            var binding = new TestBinding();

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
            component.TrySetTargetValue(binding, new MemberPathLastMember(target, new TestAccessorMemberInfo()), value, DefaultMetadata).ShouldBeTrue();
            enabledValue.ShouldEqual(canExecute);
            canExecuteHandler.ShouldNotBeNull();

            canExecute = false;
            canExecuteHandler!.Invoke(this, EventArgs.Empty);
            enabledValue.ShouldEqual(canExecute);

            component.TrySetTargetValue(binding, default, new TestValueExpression(), DefaultMetadata).ShouldBeTrue();
            enabledValue.ShouldBeTrue();
            canExecuteHandler.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleOneWayMode()
        {
            var invokeCount = 0;
            var binding = new TestBinding
            {
                UpdateSource = () => throw new NotSupportedException(),
                UpdateTarget = () => ++invokeCount
            };
            var eventHandlerBindingComponent = (EventHandlerBindingComponent.OneWay)EventHandlerBindingComponent.Get(default, true, false);
            eventHandlerBindingComponent.OnSourceLastMemberChanged(binding, null!, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            eventHandlerBindingComponent.OnSourcePathMembersChanged(binding, null!, DefaultMetadata);
            invokeCount.ShouldEqual(2);
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
            var manager = new BindingManager();
            var memberManager = new MemberManager();
            manager.AddComponent(memberManager);
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, tt, meta) => enabledMember
            });

            IEventListener? listener = null;
            var component = EventHandlerBindingComponent.Get(new BindingParameterValue(cmdParameter, null), true, true, manager);
            var binding = new TestBinding
            {
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata =>
                    {
                        return new MemberPathLastMember(this, new TestEventInfo
                        {
                            TryObserve = (o, l, arg3) =>
                            {
                                listener = l;
                                return new ActionToken((o1, o2) => listener = null);
                            }
                        });
                    }
                }
            };

            EventHandler? canExecuteHandler = null;
            var value = new TestCommand
            {
                AddCanExecuteChanged = handler => { canExecuteHandler = handler; },
                RemoveCanExecuteChanged = handler => { canExecuteHandler = null; }
            };

            ((IAttachableComponent)component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();
            component.TrySetTargetValue(binding, new MemberPathLastMember(this, new TestAccessorMemberInfo()), value, DefaultMetadata).ShouldBeTrue();
            listener.ShouldNotBeNull();
            canExecuteHandler.ShouldNotBeNull();
            component.CommandParameter.IsEmpty.ShouldBeFalse();

            ((IDetachableComponent)component).OnDetached(binding, DefaultMetadata);
            listener.ShouldBeNull();
            canExecuteHandler.ShouldBeNull();
            component.CommandParameter.IsEmpty.ShouldBeTrue();
        }

        #endregion
    }
}