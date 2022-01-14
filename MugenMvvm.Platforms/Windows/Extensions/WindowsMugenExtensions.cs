using System;
using System.Windows;
using System.Windows.Controls;
using MugenMvvm.App.Configuration;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Windows.Bindings;
using MugenMvvm.Windows.Internal;
using MugenMvvm.Windows.Presentation;
using MugenMvvm.Windows.Views;

namespace MugenMvvm.Windows.Extensions
{
    public static class WindowsMugenExtensions
    {
        public static MugenApplicationConfiguration WpfConfiguration(this MugenApplicationConfiguration configuration, bool listenAppLifecycle = true)
        {
            configuration = configuration.ServiceConfiguration<IPresenter>()
                                         .WithComponent(new WpfWindowPresenterMediator())
                                         .ServiceConfiguration<IAttachedValueManager>()
                                         .WithComponent(new DependencyObjectAttachedValueStorageProvider())
                                         .ServiceConfiguration<IBindingManager>()
                                         .WithComponent(new BindingExtensionExpressionParser())
                                         .ServiceConfiguration<IViewManager>()
                                         .WithComponent(new ItemsControlCollectionManager());

            if (listenAppLifecycle)
            {
                var app = Application.Current;
                if (app != null)
                {
                    app.Activated += WpfAppOnActivated;
                    app.Deactivated += WpfAppOnDeactivated;
                }
            }

            return configuration;
        }

        public static MugenApplicationConfiguration WpfBindingConfiguration(this MugenApplicationConfiguration configuration)
        {
            configuration.ServiceConfiguration<IMemberManager>()
                         .WithComponent(new DependencyPropertyMemberProvider());

            configuration.ServiceConfiguration<IObservationManager>()
                         .WithComponent(new DependencyPropertyObserverProvider());

            return configuration;
        }

        public static MugenApplicationConfiguration WpfAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var memberManager = configuration.GetService<IMemberManager>();
            var attachedMemberProvider = memberManager.GetAttachedMemberProvider();

            attachedMemberProvider.Register(new DependencyPropertyAccessorMemberInfo(FrameworkElement.DataContextProperty, nameof(BindableMembers.DataContext),
                typeof(FrameworkElement), MemberFlags.InstancePublic));

            var enabledMember = memberManager.TryGetMember(typeof(UIElement), MemberType.Accessor, MemberFlags.InstancePublicAll, nameof(UIElement.IsEnabled));
            if (enabledMember != null)
                attachedMemberProvider.Register(enabledMember, nameof(BindableMembers.Enabled));

            var visibilityAccessor =
                memberManager.TryGetMember(typeof(UIElement), MemberType.Accessor, MemberFlags.InstancePublicAll, nameof(UIElement.Visibility)) as IObservableMemberInfo;
            attachedMemberProvider.Register(BindableMembers.For<UIElement>()
                                                           .Visible()
                                                           .GetBuilder()
                                                           .CustomGetter((member, target, metadata) => target.Visibility == Visibility.Visible)
                                                           .CustomSetter((member, target, value, metadata) => target.Visibility = value ? Visibility.Visible : Visibility.Collapsed)
                                                           .Observable(visibilityAccessor)
                                                           .Build());
            attachedMemberProvider.Register(BindableMembers.For<ItemsControl>()
                                                           .DiffableEqualityComparer()
                                                           .GetBuilder()
                                                           .Build());

            attachedMemberProvider.Register(BindableMembers.For<FrameworkElement>()
                                                           .ElementSourceMethod()
                                                           .RawMethod
                                                           .GetBuilder()
                                                           .WithParameters(AttachedMemberBuilder.Parameter<string>("p1").Build())
                                                           .InvokeHandler((member, target, args, metadata) => target.FindName((string) args.Item!))
                                                           .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                                                           .Build());

            attachedMemberProvider.Register(BindableMembers.For<FrameworkElement>()
                                                           .ParentNative()
                                                           .GetBuilder()
                                                           .CustomGetter((_, target, _) => target.Parent)
                                                           .Observable(memberManager.TryGetMember(typeof(FrameworkElement), MemberType.Event, MemberFlags.InstancePublicAll,
                                                               nameof(FrameworkElement.Loaded)) as IObservableMemberInfo)
                                                           .Build());

            BindingMugenExtensions.RegisterViewCollectionManagerMembers<ItemsControl>(attachedMemberProvider);

            return configuration;
        }

        private static void WpfAppOnActivated(object? sender, EventArgs e)
        {
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Activating, e);
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Activated, e);
        }

        private static void WpfAppOnDeactivated(object? sender, EventArgs e)
        {
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, e);
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, e);
        }
    }
}