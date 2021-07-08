using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MugenMvvm.App.Configuration;
using MugenMvvm.Attributes;
using MugenMvvm.Avalonia.Bindings;
using MugenMvvm.Avalonia.Internal;
using MugenMvvm.Avalonia.Presentation;
using MugenMvvm.Avalonia.Views;
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

namespace MugenMvvm.Avalonia.Extensions
{
    public static class AvaloniaMugenExtensions
    {
        private static int _activeWindowCount;

        public static MugenApplicationConfiguration AvaloniaConfiguration(this MugenApplicationConfiguration configuration, bool listenAppLifecycle = true)
        {
            configuration.ServiceConfiguration<IPresenter>()
                         .WithComponent(new AvaloniaWindowPresenterMediator());

            configuration.ServiceConfiguration<IAttachedValueManager>()
                         .WithComponent(new AvaloniaObjectAttachedValueStorageProvider());

            configuration.ServiceConfiguration<IBindingManager>()
                         .WithComponent(new BindingExtensionExpressionParser());

            configuration.ServiceConfiguration<IViewManager>()
                         .WithComponent(new ItemsControlCollectionManager());

            if (listenAppLifecycle)
                WindowBase.IsActiveProperty.Changed.AddClassHandler<WindowBase>(OnActiveChanged);

            return configuration;
        }

        public static MugenApplicationConfiguration AvaloniaBindingConfiguration(this MugenApplicationConfiguration configuration)
        {
            configuration.ServiceConfiguration<IObservationManager>()
                         .WithComponent(new AvaloniaObjectObserverProvider());

            return configuration;
        }

        public static MugenApplicationConfiguration AvaloniaAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var memberManager = configuration.GetService<IMemberManager>();
            var attachedMemberProvider = memberManager.GetAttachedMemberProvider();

            attachedMemberProvider.Register(BindableMembers.For<StyledElement>()
                                                           .DataContext()
                                                           .GetBuilder()
                                                           .CustomGetter((_, target, _) => target.DataContext)
                                                           .CustomSetter((_, target, value, _) => target.DataContext = value)
                                                           .Build());

            var enabledMember = memberManager.TryGetMember(typeof(InputElement), MemberType.Accessor, MemberFlags.InstancePublicAll, nameof(InputElement.IsEnabled));
            if (enabledMember != null)
                attachedMemberProvider.Register(enabledMember, nameof(BindableMembers.Enabled));

            attachedMemberProvider.Register(BindableMembers.For<ItemsControl>()
                                                           .DiffableEqualityComparer()
                                                           .GetBuilder()
                                                           .Build());

            attachedMemberProvider.Register(BindableMembers.For<IControl>()
                                                           .ElementSourceMethod()
                                                           .RawMethod
                                                           .GetBuilder()
                                                           .WithParameters(AttachedMemberBuilder.Parameter<string>("p1").Build())
                                                           .InvokeHandler((member, target, args, metadata) =>
                                                           {
                                                               var nameScope = target.FindNameScope();
                                                               return nameScope?.Find<IControl>((string)args.Item!);
                                                           })
                                                           .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                                                           .Build());

            attachedMemberProvider.Register(BindableMembers.For<IStyledElement>()
                                                           .ParentNative()
                                                           .GetBuilder()
                                                           .CustomGetter((_, target, _) => target.Parent)
                                                           .Observable(memberManager.TryGetMember(typeof(IStyledElement), MemberType.Accessor,
                                                               MemberFlags.InstancePublic, nameof(IStyledElement.Parent)) as IObservableMemberInfo)
                                                           .Build());

            BindingMugenExtensions.RegisterViewCollectionManagerMembers<ItemsControl>(attachedMemberProvider, nameof(ItemsControl.Items));

            return configuration;
        }

        [Preserve(Conditional = true)]
        public static void RaisePropertyChanged(this MemberListenerCollection collection, object? sender, AvaloniaPropertyChangedEventArgs args) =>
            collection.Raise(sender, args, args.Property.Name, null);

        private static void OnActiveChanged(WindowBase owner, AvaloniaPropertyChangedEventArgs args)
        {
            var newValue = (bool?)args.NewValue;
            if (newValue.GetValueOrDefault())
            {
                if (Interlocked.Increment(ref _activeWindowCount) == 1)
                {
                    MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Activating, args);
                    MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Activated, args);
                }
            }
            else
            {
                if (Interlocked.Decrement(ref _activeWindowCount) == 0)
                {
                    MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, args);
                    MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, args);
                }
            }
        }
    }
}