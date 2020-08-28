using System;
using System.Threading;
using Foundation;
using JetBrains.Annotations;
using MugenMvvm.App.Configuration;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Ios.App;
using MugenMvvm.Ios.Constants;
using MugenMvvm.Ios.Internal;
using MugenMvvm.Ios.Members;
using MugenMvvm.Ios.Navigation;
using MugenMvvm.Ios.Presenters;
using MugenMvvm.Ios.Views;
using MugenMvvm.Threading.Components;
using ObjCRuntime;
using UIKit;

namespace MugenMvvm.Ios.Extensions
{
    public static partial class MugenIosExtensions
    {
        #region Methods

        public static MugenApplicationConfiguration IosConfiguration(this MugenApplicationConfiguration configuration, bool shouldSaveAppState = true)
        {
            configuration
                .ServiceConfiguration<IThreadDispatcher>()
                .WithComponent(new SynchronizationContextThreadDispatcher(SynchronizationContext.Current));

            configuration
                .ServiceConfiguration<IWeakReferenceManager>()
                .WithComponent(new IosWeakReferenceProvider());

            configuration
                .ServiceConfiguration<IAttachedValueManager>()
                .WithComponent(new IosAttachedValueStorageProvider());

            configuration
                .ServiceConfiguration<INavigationDispatcher>()
                .WithComponent(new IosConditionNavigationDispatcher());

            if (shouldSaveAppState)
            {
                var applicationStateDispatcher = new IosApplicationStateDispatcher();
                configuration.Application.AddComponent(applicationStateDispatcher);
                configuration
                    .ServiceConfiguration<IViewManager>()
                    .WithComponent(applicationStateDispatcher);
            }

            configuration
                .ServiceConfiguration<IViewManager>()
                .WithComponent(new IosViewLifecycleDispatcher());

            return configuration;
        }

        public static MugenApplicationConfiguration IosRootViewModel(this MugenApplicationConfiguration configuration, Type rootViewModelType, bool wrapToNavigationController = true)
        {
            configuration
                .ServiceConfiguration<IPresenter>()
                .WithComponent(new ApplicationPresenter(rootViewModelType, wrapToNavigationController));
            return configuration;
        }

        public static MugenApplicationConfiguration IosAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var memberManager = configuration.ServiceConfiguration<IMemberManager>().Service();
            var attachedMemberProvider = memberManager.GetOrAddComponent(context => new AttachedMemberProvider());

            //UIViewController
            attachedMemberProvider.Register(BindableMembers
                .For<UIViewController>()
                .ParentNative()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.ParentViewController ?? target.PresentingViewController)
                .Observable()
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
                .Observable()
                .Build());
            attachedMemberProvider.Register(BindableMembers
                .For<UIView>()
                .ContentTemplateSelector()
                .GetBuilder()
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
                    var recognizer = new UITapGestureRecognizer(closure, ClickClosure.OnClickSelector) { NumberOfTapsRequired = 1 };
                    target.UserInteractionEnabled = true;
                    target.AddGestureRecognizer(recognizer);
                    return new ActionToken((t, r) => ((UIView)t!).RemoveGestureRecognizer((UIGestureRecognizer)r!), target, recognizer);
                })
                .Build());

            //UIControl
            var clickEvent = (IObservableMemberInfo?)memberManager.TryGetMember(typeof(UIControl), MemberType.Event, MemberFlags.All, nameof(UIControl.TouchUpInside));
            if (clickEvent != null)
            {
                attachedMemberProvider.Register(BindableMembers
                    .For<UIView>()
                    .Click()
                    .GetBuilder()
                    .WrapMember(clickEvent)
                    .Build());
            }

            var valueChangedEvent = (IObservableMemberInfo?)memberManager.TryGetMember(typeof(UIControl), MemberType.Event, MemberFlags.All, nameof(UIControl.ValueChanged));
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
            var attachedValues = MugenService.AttachedValueManager.TryGetAttachedValues(target, metadata);
            if (attachedValues.Remove(IosInternalConstants.ContentViewControllerPath, out var v) && v is UIViewController controller)
                controller.RemoveFromParentViewController();

            var selector = target.BindableMembers().ContentTemplateSelector();
            if (selector != null)
                newValue = selector.SelectTemplate(target, newValue);

            if (newValue is UIViewController viewController)
            {
                var currentController = MugenBindingExtensions.TryFindParent<UIViewController>(target, metadata);
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
            if (!(newValue is UIView content))
            {
                ExceptionManager.ThrowNotValidArgument(nameof(content));
                return;
            }

            content.Frame = target.Frame;
            content.AutoresizingMask = UIViewAutoresizing.All;
            target.AddSubviewNotifyParent(content);
        }

        #endregion

        #region Nested types

        private sealed class TextChangedObserver : NSObject
        {
            #region Fields

            public static readonly INotifiableMemberInfo TextChangedMember = AttachedMemberBuilder
                .Event<UIView>(nameof(UITextField.Text) + BindingInternalConstant.ChangedEventPostfix)
                .Build();

            public static readonly Selector OnTextChangedSelector = new Selector("t:");
            public static readonly TextChangedObserver Instance = new TextChangedObserver();

            #endregion

            #region Constructors

            private TextChangedObserver()
            {
            }

            #endregion

            #region Methods

            [Export("t:")]
            [UsedImplicitly]
            private void OnTextChanged(NSNotification notification) => TextChangedMember.Raise(notification.Object, EventArgs.Empty);

            #endregion
        }

        private sealed class ClickClosure : NSObject
        {
            #region Fields

            private readonly WeakEventListener _eventListener;
            public static readonly Selector OnClickSelector = new Selector("c:");

            #endregion

            #region Constructors

            public ClickClosure(WeakEventListener eventListener)
            {
                _eventListener = eventListener;
            }

            #endregion

            #region Methods

            [Export("c:")]
            [UsedImplicitly]
            private void OnClick(UITapGestureRecognizer recognizer)
            {
                if (!_eventListener.TryHandle(recognizer.View, EventArgs.Empty, null))
                    recognizer.View.RemoveGestureRecognizer(recognizer);
            }

            #endregion
        }

        #endregion
    }
}