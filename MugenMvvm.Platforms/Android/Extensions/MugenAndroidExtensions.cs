using Android.App;
using Android.Content;
using Android.Views;
using MugenMvvm.Android.Binding;
using MugenMvvm.Android.Collections;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Presenters;
using MugenMvvm.Android.Views;
using MugenMvvm.App.Configuration;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Threading.Components;
using IViewManager = MugenMvvm.Interfaces.Views.IViewManager;

namespace MugenMvvm.Android.Extensions
{
    partial class MugenAndroidExtensions
    {
        #region Methods

        private static MugenApplicationConfiguration AndroidConfiguration(this MugenApplicationConfiguration configuration, Context? context = null)
        {
            // MugenAndroidNativeService.Initialize(context ?? Application.Context, new AndroidViewBindCallback());
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

        private static MugenApplicationConfiguration AndroidConfigurationBase(this MugenApplicationConfiguration configuration)
        {
            MugenAndroidNativeService.AddLifecycleDispatcher(new AndroidNativeViewLifecycleDispatcher());
            configuration.ServiceConfiguration<IThreadDispatcher>()
                .WithComponent(new SynchronizationContextThreadDispatcher(Application.SynchronizationContext));

            configuration.ServiceConfiguration<IViewManager>()
                .WithComponent(new AndroidViewStateDispatcher())
                .WithComponent(new AndroidViewFirstInitializer())
                .WithComponent(new AndroidViewRequestManager())
                .WithComponent(new AndroidDestroyViewHandler())
                .WithComponent(new AndroidViewMappingDecorator());

            var mediatorPresenter = configuration.ServiceConfiguration<IPresenter>().Service().GetOrAddComponent(ctx => new ViewModelPresenter());
            mediatorPresenter.RegisterMediator<ActivityViewModelPresenterMediator, IActivityView>();
            return configuration;
        }

        #endregion
    }
}