using Android.App;
using Android.Content;
using Android.Views;
using MugenMvvm.Android.Binding;
using MugenMvvm.Android.Collections;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Observation;
using MugenMvvm.Android.Presenters;
using MugenMvvm.Android.Views;
using MugenMvvm.App.Configuration;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Threading.Components;
using IViewManager = MugenMvvm.Interfaces.Views.IViewManager;

namespace MugenMvvm.Android.Extensions
{
    public static class MugenAndroidExtensions
    {
        #region Methods

        public static MugenApplicationConfiguration AndroidConfiguration(this MugenApplicationConfiguration configuration, bool nativeConfiguration, Context? context = null)
        {
            MugenAndroidNativeService.Initialize(context ?? Application.Context, nativeConfiguration, new AndroidViewBindCallback());
            MugenAndroidNativeService.AddLifecycleDispatcher(new AndroidNativeViewLifecycleDispatcher());
            configuration.ServiceConfiguration<IThreadDispatcher>()
                .WithComponent(new SynchronizationContextThreadDispatcher(Application.SynchronizationContext));

            configuration.ServiceConfiguration<IViewManager>()
                .WithComponent(new AndroidViewStateDispatcher())
                .WithComponent(new AndroidViewFirstInitializer())
                .WithComponent(new AndroidViewMappingDecorator());

            var mediatorPresenter = configuration.ServiceConfiguration<IPresenter>().Service().GetOrAddComponent(ctx => new ViewModelMediatorPresenter());
            mediatorPresenter.RegisterMediator<ActivityViewModelPresenterMediator, IActivityView>();

            return configuration;
        }

        public static MugenApplicationConfiguration AndroidMenuAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var attachedMemberProvider = configuration.ServiceConfiguration<IMemberManager>().Service().GetOrAddComponent(context => new AttachedMemberProvider());
            attachedMemberProvider.Register(BindableMembers.For<IMenu>()
                .ItemTemplate()
                .GetBuilder()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IMenu>()
                .ItemsSource()
                .GetBuilder()
                .PropertyChangedHandler((member, target, value, newValue, metadata) => AndroidMenuItemsSourceAdapter.GetOrAdd(target, target.BindableMembers().ItemTemplate()!).Attach(newValue))
                .Build());

            var enabled = AttachedMemberBuilder
                .Property<IMenuItem, bool>(nameof(IMenuItem.IsEnabled))
                .CustomGetter((member, target, metadata) => target.IsEnabled)
                .CustomSetter((member, target, value, metadata) => target.SetEnabled(value))
                .Build();
            attachedMemberProvider.Register(enabled);
            attachedMemberProvider.Register(enabled, BindableMembers.For<object>().Enabled());
            attachedMemberProvider.Register(AttachedMemberBuilder
                .Property<IMenuItem, bool>(nameof(IMenuItem.IsCheckable))
                .CustomGetter((member, target, metadata) => target.IsCheckable)
                .CustomSetter((member, target, value, metadata) => target.SetCheckable(value))
                .Build());
            attachedMemberProvider.Register(AttachedMemberBuilder
                .Property<IMenuItem, bool>(nameof(IMenuItem.IsChecked))
                .CustomGetter((member, target, metadata) => target.IsChecked)
                .CustomSetter((member, target, value, metadata) => target.SetChecked(value))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IMenuItem>()
                .Title()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.TitleFormatted?.ToString())
                .CustomSetter((member, target, value, metadata) => target.SetTitle(value!))
                .Build());
            attachedMemberProvider.Register(AttachedMemberBuilder
                .Property<IMenuItem, bool>(nameof(IMenuItem.IsVisible))
                .CustomGetter((member, target, metadata) => target.IsVisible)
                .CustomSetter((member, target, value, metadata) => target.SetVisible(value))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IMenuItem>()
                .Click()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => MenuItemClickListener.AddListener(target, listener))
                .Build());
            return configuration;
        }

        public static MugenApplicationConfiguration AndroidNativeAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var attachedMemberProvider = configuration.ServiceConfiguration<IMemberManager>().Service().GetOrAddComponent(context => new AttachedMemberProvider());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .Parent()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Parent)
                .ObservableHandler((member, target, listener, metadata) => AndroidViewMemberObserver.Add(target, listener, nameof(target.Parent)))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .Visible()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Visibility == (int)ViewStates.Visible)
                .CustomSetter((member, target, value, metadata) => target.Visibility = value ? (int)ViewStates.Visible : (int)ViewStates.Gone)
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .Invisible()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Visibility == (int)ViewStates.Invisible)
                .CustomSetter((member, target, value, metadata) => target.Visibility = value ? (int)ViewStates.Invisible : (int)ViewStates.Gone)
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .RelativeSourceMethod()
                .RawMethod
                .GetBuilder()
                .WithParameters(AttachedMemberBuilder.Parameter<string>("p1").Build(), AttachedMemberBuilder.Parameter<string>("p1").DefaultValue(BoxingExtensions.Box(1)).Build())
                .InvokeHandler((member, target, args, metadata) => target.FindRelativeSource((string)args[0]!, (int)args[1]!))
                .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .ParentChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberObserver.Add(target, listener, member.Name))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .Click()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberObserver.Add(target, listener, member.Name))
                .Build());

            attachedMemberProvider.Register(BindableMembers.For<ITextView>()
                .TextChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberObserver.Add(target, listener, member.Name))
                .Build());

            attachedMemberProvider.Register(BindableMembers.For<IRefreshView>()
                .Refreshed()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberObserver.Add(target, listener, member.Name))
                .Build());

            attachedMemberProvider.Register(BindableMembers.For<IHasMenuView>()
                .MenuTemplate()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    oldValue?.Clear(target.Menu);
                    newValue?.Apply(target.Menu, target);
                })
                .Build());

            attachedMemberProvider.Register(BindableMembers.For<IListView>()
                .StableIdProvider()
                .GetBuilder()
                .NonObservable()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IListView>()
                .ItemTemplateSelector()
                .GetBuilder()
                .NonObservable()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IListView>()
                .ItemsSource()
                .GetBuilder()
                .PropertyChangedHandler((member, target, value, newValue, metadata) =>
                {
                    if (!(target.ItemsSourceProvider is AndroidCollectionItemsSourceProvider provider))
                    {
                        provider = new AndroidCollectionItemsSourceProvider(target, target.BindableMembers().ItemTemplateSelector()!, target.BindableMembers().StableIdProvider());
                        target.ItemsSourceProvider = provider;
                    }
                    provider.SetItemsSource(newValue);
                }).Build());
            return configuration;
        }

        #endregion
    }
}