using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MugenMvvm.App.Configuration;
using MugenMvvm.Attributes;
using MugenMvvm.Avalonia.Bindings;
using MugenMvvm.Avalonia.Collections;
using MugenMvvm.Avalonia.Internal;
using MugenMvvm.Avalonia.Presentation;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Presentation;

namespace MugenMvvm.Avalonia.Extensions
{
    public static class AvaloniaMugenExtensions
    {
        public static MugenApplicationConfiguration AvaloniaConfiguration(this MugenApplicationConfiguration configuration)
        {
            configuration.ServiceConfiguration<IPresenter>()
                         .WithComponent(new AvaloniaWindowPresenterMediator());

            configuration.ServiceConfiguration<IAttachedValueManager>()
                         .WithComponent(new AvaloniaObjectAttachedValueStorageProvider());

            configuration.ServiceConfiguration<IBindingManager>()
                         .WithComponent(new BindingExtensionExpressionParser());

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
            var memberManager = configuration.ServiceConfiguration<IMemberManager>().Service;
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
                                                           .NonObservable()
                                                           .PropertyChangedHandler((_, target, _, newValue, _) =>
                                                               ObservableCollectionAdapter.GetOrAdd(target).Adapter.DiffableComparer = newValue)
                                                           .Build());

            attachedMemberProvider.Register(BindableMembers.For<IControl>()
                                                           .ElementSourceMethod()
                                                           .RawMethod
                                                           .GetBuilder()
                                                           .WithParameters(AttachedMemberBuilder.Parameter<string>("p1").Build())
                                                           .InvokeHandler((member, target, args, metadata) =>
                                                           {
                                                               var nameScope = target.FindNameScope();
                                                               return nameScope?.Find<IControl>((string) args.Item!);
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

            attachedMemberProvider.Register(AttachedMemberBuilder.Property<ItemsControl, IEnumerable?>(nameof(ItemsControl.Items))
                                                                 .CustomGetter((_, target, _) => ObservableCollectionAdapter.GetItemsSource(target.Items))
                                                                 .CustomSetter((member, target, value, metadata) =>
                                                                 {
                                                                     if (!ReferenceEquals(member.GetValue(target, metadata), value))
                                                                         ObservableCollectionAdapter.GetOrAdd(target).Adapter.Collection = value;
                                                                 })
                                                                 .Build());
            return configuration;
        }

        [Preserve(Conditional = true)]
        public static void RaisePropertyChanged(this MemberListenerCollection collection, object? sender, AvaloniaPropertyChangedEventArgs args) =>
            collection.Raise(sender, args, args.Property.Name, null);
    }
}