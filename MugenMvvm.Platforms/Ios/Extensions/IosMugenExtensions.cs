﻿using System;
using Foundation;
using JetBrains.Annotations;
using MugenMvvm.App.Configuration;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Ios.App;
using MugenMvvm.Ios.Bindings;
using MugenMvvm.Ios.Constants;
using MugenMvvm.Ios.Internal;
using MugenMvvm.Ios.Navigation;
using MugenMvvm.Ios.Presentation;
using MugenMvvm.Ios.Views;
using ObjCRuntime;
using UIKit;

namespace MugenMvvm.Ios.Extensions
{
    public static partial class IosMugenExtensions
    {
        public static MugenApplicationConfiguration IosConfiguration(this MugenApplicationConfiguration configuration, bool shouldSaveAppState = true)
        {
            configuration
                .ServiceConfiguration<IWeakReferenceManager>()
                .WithComponent(new IosWeakReferenceProvider());

            configuration
                .ServiceConfiguration<IAttachedValueManager>()
                .WithComponent(new IosAttachedValueStorageProvider());

            configuration
                .ServiceConfiguration<INavigationDispatcher>()
                .WithComponent(new ViewNavigationConditionDispatcher());

            if (shouldSaveAppState)
            {
                var applicationStateDispatcher = new ApplicationStateDispatcher();
                configuration.Application.AddComponent(applicationStateDispatcher);
                configuration
                    .ServiceConfiguration<IViewManager>()
                    .WithComponent(applicationStateDispatcher);
            }

            configuration
                .ServiceConfiguration<IViewManager>()
                .WithComponent(new ViewLifecycleDispatcher())
                .WithComponent(new ViewCollectionManager());

            configuration
                .ServiceConfiguration<IPresenter>()
                .WithComponent(new ModalViewPresenterMediator());

            return configuration;
        }

        public static MugenApplicationConfiguration IosRootViewModel(this MugenApplicationConfiguration configuration, Type rootViewModelType,
            bool wrapToNavigationController = true)
        {
            configuration
                .ServiceConfiguration<IPresenter>()
                .WithComponent(new ApplicationPresenter(rootViewModelType, wrapToNavigationController));
            return configuration;
        }

        public static MugenApplicationConfiguration IosAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var memberManager = configuration.GetService<IMemberManager>();
            var attachedMemberProvider = memberManager.GetAttachedMemberProvider();

            //NSObject
            BindingMugenExtensions.RegisterViewCollectionManagerMembers<NSObject>(attachedMemberProvider);

            //UIViewController
            attachedMemberProvider.Register(BindableMembers
                                            .For<UIViewController>()
                                            .ParentNative()
                                            .GetBuilder()
                                            .CustomGetter((member, target, metadata) => target.ParentViewController ?? target.PresentingViewController)
                                            .ObservableAutoHandler()
                                            .Build());

            //UIView
            attachedMemberProvider.Register(BindableMembers
                                            .For<UIView>()
                                            .ParentNative()
                                            .GetBuilder()
                                            .CustomGetter((member, target, metadata) =>
                                            {
                                                if (!target.IsAlive())
                                                    return null;
                                                if (target.NextResponder is UIViewController controller && Equals(target, controller.View))
                                                    return controller;
                                                return target.Superview;
                                            })
                                            .ObservableAutoHandler()
                                            .Build());
            attachedMemberProvider.Register(BindableMembers
                                            .For<UIView>()
                                            .ContentTemplateSelector()
                                            .GetBuilder()
                                            .DefaultValue(DefaultContentTemplateSelector.Instance)
                                            .NonObservable()
                                            .Build());
            attachedMemberProvider.Register(BindableMembers
                                            .For<UIView>()
                                            .ContentSetter()
                                            .GetBuilder()
                                            .NonObservable()
                                            .Build());
            attachedMemberProvider.Register(BindableMembers
                                            .For<UIView>()
                                            .Content()
                                            .GetBuilder()
                                            .PropertyChangedHandler(OnContentChanged)
                                            .Build());
            attachedMemberProvider.Register(BindableMembers
                                            .For<UIView>()
                                            .Visible()
                                            .GetBuilder()
                                            .CustomGetter((member, target, metadata) => !target.Hidden)
                                            .CustomSetter((member, target, value, metadata) => target.Hidden = !value)
                                            .NonObservable()
                                            .Build());
            attachedMemberProvider.Register(BindableMembers.For<UIView>()
                                                           .Click()
                                                           .GetBuilder()
                                                           .CustomImplementation((member, target, listener, metadata) =>
                                                           {
                                                               var closure = new ClickClosure(listener.ToWeak());
                                                               var recognizer = new UITapGestureRecognizer(closure, ClickClosure.OnClickSelector) {NumberOfTapsRequired = 1};
                                                               target.UserInteractionEnabled = true;
                                                               target.AddGestureRecognizer(recognizer);
                                                               return ActionToken.FromDelegate((t, r) =>
                                                               {
                                                                   var v = (UIView?) ((IWeakReference) t!).Target;
                                                                   var g = (UITapGestureRecognizer?) ((IWeakReference) r!).Target;
                                                                   if (v != null && g != null)
                                                                       v.RemoveGestureRecognizer(g);
                                                               }, target.ToWeakReference(), recognizer.ToWeakReference());
                                                           })
                                                           .Build());
            attachedMemberProvider.Register(BindableMembers
                                            .For<UIView>()
                                            .ItemTemplateSelector()
                                            .GetBuilder()
                                            .NonObservable()
                                            .Build());

            //UIControl
            var clickEvent = (IObservableMemberInfo?) memberManager.TryGetMember(typeof(UIControl), MemberType.Event, MemberFlags.All, nameof(UIControl.TouchUpInside));
            if (clickEvent != null)
            {
                attachedMemberProvider.Register(BindableMembers
                                                .For<UIControl>()
                                                .Click()
                                                .GetBuilder()
                                                .WrapMember(clickEvent)
                                                .Build());
            }

            var valueChangedEvent = (IObservableMemberInfo?) memberManager.TryGetMember(typeof(UIControl), MemberType.Event, MemberFlags.All, nameof(UIControl.ValueChanged));
            if (valueChangedEvent != null)
            {
                attachedMemberProvider.Register(valueChangedEvent, nameof(UISwitch.On) + BindingInternalConstant.ChangedEventPostfix);
                attachedMemberProvider.Register(valueChangedEvent, nameof(UIDatePicker.Date) + BindingInternalConstant.ChangedEventPostfix);
                attachedMemberProvider.Register(valueChangedEvent, nameof(UISegmentedControl.SelectedSegment) + BindingInternalConstant.ChangedEventPostfix);
                attachedMemberProvider.Register(valueChangedEvent, nameof(UIRefreshControl.Refreshing) + BindingInternalConstant.ChangedEventPostfix);
                attachedMemberProvider.Register(valueChangedEvent, nameof(UIPageControl.CurrentPage) + BindingInternalConstant.ChangedEventPostfix);
                attachedMemberProvider.Register(valueChangedEvent, nameof(IosBindableMembers.Refreshed));
            }

            //UIButton
            attachedMemberProvider.Register(AttachedMemberBuilder
                                            .Property<UIButton, string>(nameof(UIButton.Title))
                                            .CustomGetter((member, target, metadata) => target.CurrentTitle)
                                            .CustomSetter((member, target, value, metadata) => target.SetTitle(value, UIControlState.Normal))
                                            .Build());

            //UITextField, UITextView
            //note using raw string instead of UITextField.TextFieldTextDidChangeNotification, UITextView.TextDidChangeNotification
            attachedMemberProvider.Register(TextChangedObserver.TextChangedMember);
            var textChangedNotification = new NSString("UITextFieldTextDidChangeNotification");
            NSNotificationCenter.DefaultCenter.AddObserver(TextChangedObserver.Instance, TextChangedObserver.OnTextChangedSelector, textChangedNotification, null);
            textChangedNotification.Dispose();

            textChangedNotification = new NSString("UITextViewTextDidChangeNotification");
            NSNotificationCenter.DefaultCenter.AddObserver(TextChangedObserver.Instance, TextChangedObserver.OnTextChangedSelector, textChangedNotification, null);
            textChangedNotification.Dispose();

            return configuration;
        }

        private static void OnContentChanged(IAccessorMemberInfo member, UIView target, object? oldValue, object? newValue, IReadOnlyMetadataContext? metadata)
        {
            var attachedValues = target.AttachedValues(metadata);
            if (attachedValues.Remove(IosInternalConstants.ContentViewControllerPath, out var v) && v is UIViewController controller)
                controller.RemoveFromParentViewController();

            var selector = target.BindableMembers().ContentTemplateSelector();
            if (selector != null)
                newValue = selector.SelectTemplate(target, newValue);

            if (newValue is UIViewController viewController)
            {
                var currentController = BindingMugenExtensions.TryFindParent<UIViewController>(target, metadata);
                if (currentController != null)
                {
                    attachedValues.Set(IosInternalConstants.ContentViewControllerPath, viewController, out _);
                    viewController.WillMoveToParentViewController(currentController);
                    currentController.AddChildViewController(viewController);
                    viewController.DidMoveToParentViewController(currentController);
                    newValue = viewController.View;
                }
            }

            var contentSetter = target.BindableMembers().ContentSetter();
            if (contentSetter != null)
            {
                contentSetter(target, newValue);
                return;
            }

            target.ClearSubViews();
            if (newValue is not UIView content)
            {
                ExceptionManager.ThrowNotValidArgument(nameof(content));
                return;
            }

            content.Frame = target.Frame;
            content.AutoresizingMask = UIViewAutoresizing.All;
            target.AddSubviewNotifyParent(content);
        }

        private sealed class TextChangedObserver : NSObject
        {
            public static readonly INotifiableMemberInfo TextChangedMember = AttachedMemberBuilder
                                                                             .Event<UIView>(nameof(UITextField.Text) + BindingInternalConstant.ChangedEventPostfix)
                                                                             .Build();

            public static readonly Selector OnTextChangedSelector = new("t:");
            public static readonly TextChangedObserver Instance = new();

            private TextChangedObserver()
            {
            }

            [Export("t:")]
            [UsedImplicitly]
            private void OnTextChanged(NSNotification notification) => TextChangedMember.Raise(notification.Object, EventArgs.Empty);
        }

        private sealed class ClickClosure : NSObject
        {
            public static readonly Selector OnClickSelector = new("c:");

            private readonly WeakEventListener _eventListener;

            public ClickClosure(WeakEventListener eventListener)
            {
                _eventListener = eventListener;
            }

            [Export("c:")]
            [UsedImplicitly]
            private void OnClick(UITapGestureRecognizer recognizer)
            {
                if (!_eventListener.TryHandle(recognizer.View, EventArgs.Empty, null))
                    recognizer.View.RemoveGestureRecognizer(recognizer);
            }
        }
    }
}