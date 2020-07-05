using Android.App;
using Android.Content;
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
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Threading.Components;

namespace MugenMvvm.Android.Extensions
{
    public static class MugenAndroidExtensions
    {
        #region Methods

        public static MugenApplicationConfiguration AndroidConfiguration(this MugenApplicationConfiguration configuration, bool nativeConfiguration, bool includeSupportLibs, Context? context = null)
        {
            MugenAndroidNativeService.Initialize(context ?? Application.Context);
            MugenAndroidNativeService.AddLifecycleDispatcher(new AndroidNativeViewLifecycleDispatcher(), nativeConfiguration);
            if (nativeConfiguration)
                MugenAndroidNativeService.NativeConfiguration(new AndroidViewBindCallback(), includeSupportLibs);
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