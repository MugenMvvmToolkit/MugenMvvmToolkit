using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components.Binding;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Commands.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components.Binding
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnAttachedShouldUpdateTargetAddOneTimeModeIfNeed(bool addOneTimeMode)
        {
            var updateCount = 0;
            var binding = new TestBinding
            {
                UpdateTarget = () => ++updateCount,
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                }
            };
            if (addOneTimeMode)
                binding.Source = new TestMemberPathObserver {GetLastMember = metadata => default};
            IAttachableComponent component = EventHandlerBindingComponent.Get(default, false, true);
            component.OnAttached(binding, DefaultMetadata);
            updateCount.ShouldEqual(1);
            if (addOneTimeMode)
            {
                binding.Components.Count.ShouldEqual(1);
                binding.Components.Get<object>().Single().ShouldEqual(OneTimeBindingMode.NonDisposeInstance);
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

            var components = new List<IDisposable>();
            for (var i = 0; i < count; i++)
            {
                var subscribe = MugenService.AddComponent(new TestBindingEventHandlerComponent(MugenBindingService.BindingManager)
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
                components.Add(subscribe);
            }

            var component = EventHandlerBindingComponent.Get(default, false, true);
            ((IAttachableComponent) component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();

            component.TrySetTargetValue(binding, default, new TestValueExpression(), DefaultMetadata);
            eventListener!.TryHandle(sender, message, DefaultMetadata).ShouldBeTrue();
            if (isError)
                errorEventCount.ShouldEqual(count);
            else
                beginEventCount.ShouldEqual(count);
            endEventCount.ShouldEqual(count);
            components.ForEach(disposable => disposable.Dispose());
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

            ((IAttachableComponent) component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();
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

            ((IAttachableComponent) component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();
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
                    enabledValue = (bool) o1!;
                }
            };
            using var _ = MugenService.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    meta.ShouldEqual(DefaultMetadata);
                    r.ShouldEqual(BindableMembers.For<object>().Enabled().Name);
                    f.ShouldEqual(MemberFlags.All & ~(MemberFlags.NonPublic | MemberFlags.Static));
                    m.ShouldEqual(MemberType.Accessor);
                    t.ShouldEqual(target.GetType());
                    return enabledMember;
                }
            });

            var component = EventHandlerBindingComponent.Get(new BindingParameterValue(cmdParameter, null), true, true);
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
            var eventHandlerBindingComponent = (EventHandlerBindingComponent.OneWay) EventHandlerBindingComponent.Get(default, true, false);
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

            using var _ = MugenService.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => enabledMember
            });

            IEventListener? listener = null;
            var component = EventHandlerBindingComponent.Get(new BindingParameterValue(cmdParameter, null), true, true);
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

            ((IAttachableComponent) component).OnAttaching(binding, DefaultMetadata).ShouldBeTrue();
            component.TrySetTargetValue(binding, new MemberPathLastMember(this, new TestAccessorMemberInfo()), value, DefaultMetadata).ShouldBeTrue();
            listener.ShouldNotBeNull();
            canExecuteHandler.ShouldNotBeNull();
            component.CommandParameter.IsEmpty.ShouldBeFalse();

            ((IDetachableComponent) component).OnDetached(binding, DefaultMetadata);
            listener.ShouldBeNull();
            canExecuteHandler.ShouldBeNull();
            component.CommandParameter.IsEmpty.ShouldBeTrue();
        }

        #endregion
    }
}