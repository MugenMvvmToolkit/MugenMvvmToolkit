using Android.App;
using Android.Content;
using Android.Views;
using Java.Lang;
using MugenMvvm.Android.Binding;
using MugenMvvm.Android.Collections;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Internal;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Observation;
using MugenMvvm.App.Configuration;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;

namespace MugenMvvm.Android.Extensions
{
    public static partial class MugenAndroidExtensions
    {
        #region Methods

        public static MugenApplicationConfiguration AndroidNativeConfiguration(this MugenApplicationConfiguration configuration, Context? context = null)
        {
            MugenAndroidNativeService.InitializeNative(context ?? Application.Context, new AndroidViewBindCallback(), new AndroidNativeWeakReferenceCallback());
            return configuration.AndroidConfigurationBase();
        }

        public static MugenApplicationConfiguration AndroidNativeAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var attachedMemberProvider = configuration.ServiceConfiguration<IMemberManager>().Service().GetOrAddComponent(context => new AttachedMemberProvider());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .Parent()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Parent)
                .CustomSetter((member, target, value, metadata) => target.Parent = (Object) value!)
                .ObservableHandler((member, target, listener, metadata) => AndroidViewMemberObserver.Add(target, listener, nameof(target.Parent)))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .Visible()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Visibility == (int) ViewStates.Visible)
                .CustomSetter((member, target, value, metadata) => target.Visibility = value ? (int) ViewStates.Visible : (int) ViewStates.Gone)
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .Invisible()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Visibility == (int) ViewStates.Invisible)
                .CustomSetter((member, target, value, metadata) => target.Visibility = value ? (int) ViewStates.Invisible : (int) ViewStates.Gone)
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IAndroidView>()
                .RelativeSourceMethod()
                .RawMethod
                .GetBuilder()
                .WithParameters(AttachedMemberBuilder.Parameter<string>("p1").Build(), AttachedMemberBuilder.Parameter<string>("p1").DefaultValue(BoxingExtensions.Box(1)).Build())
                .InvokeHandler((member, target, args, metadata) => target.FindRelativeSource((string) args[0]!, (int) args[1]!))
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


            attachedMemberProvider.Register(BindableMembers.For<IContentView>()
                .ContentTemplateSelector()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    if (newValue is IDataTemplateSelector selector)
                        member.SetValue(target, new ContentTemplateResourceSelectorWrapper(selector), metadata);
                })
                .NonObservable()
                .DefaultValue(new DefaultContentTemplate())
                .Build());

            attachedMemberProvider.Register(BindableMembers.For<IContentView>()
                .Content()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Content?.BindableMembers().DataContext())
                .CustomSetter((member, target, value, metadata) =>
                {
                    var contentTemplateSelector = target.BindableMembers().ContentTemplateSelector();
                    if (contentTemplateSelector == null)
                        ExceptionManager.ThrowNotSupported(nameof(contentTemplateSelector));

                    var oldValue = target.Content;
                    var newValue = value == null ? null : (Object) contentTemplateSelector.SelectTemplate(target, value)!;
                    if (Equals(newValue, oldValue))
                        return;

                    var viewManager = MugenService.ViewManager;
                    if (oldValue != null)
                        viewManager.OnLifecycleChanged(oldValue, ViewLifecycleState.Disappearing, metadata, metadata);

                    if (newValue != null)
                    {
                        newValue.BindableMembers().SetDataContext(value);
                        newValue.BindableMembers().SetParent(target);
                        viewManager.OnLifecycleChanged(newValue, ViewLifecycleState.Appearing, metadata, metadata);
                    }

                    target.Content = newValue!;

                    if (oldValue != null)
                        viewManager.OnLifecycleChanged(oldValue, ViewLifecycleState.Disappeared, metadata, metadata);

                    if (newValue != null)
                        viewManager.OnLifecycleChanged(newValue, ViewLifecycleState.Appeared, metadata, metadata);
                })
                .Observable()
                .Build());


            attachedMemberProvider.Register(BindableMembers.For<IListView>()
                .StableIdProvider()
                .GetBuilder()
                .NonObservable()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IListView>()
                .ItemTemplateSelector()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    if (target.ProviderType == ListView.ContentProviderType && newValue is IDataTemplateSelector selector)
                        member.SetValue(target, new ContentTemplateResourceSelectorWrapper(selector), metadata);
                })
                .NonObservable()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<IListView>()
                .ItemsSource()
                .GetBuilder()
                .PropertyChangedHandler((member, target, value, newValue, metadata) =>
                {
                    if (target.ProviderType == ListView.ContentProviderType)
                    {
                        if (!(target.ItemsSourceProvider is AndroidContentItemsSourceProvider provider))
                        {
                            provider = new AndroidContentItemsSourceProvider(target, (IContentTemplateSelector) target.BindableMembers().ItemTemplateSelector()!);
                            target.ItemsSourceProvider = provider;
                        }

                        provider.SetItemsSource(newValue);
                    }
                    else
                    {
                        if (!(target.ItemsSourceProvider is AndroidCollectionItemsSourceProvider provider))
                        {
                            provider = new AndroidCollectionItemsSourceProvider(target, (IDataTemplateSelector) target.BindableMembers().ItemTemplateSelector()!, target.BindableMembers().StableIdProvider());
                            target.ItemsSourceProvider = provider;
                        }

                        provider.SetItemsSource(newValue);
                    }
                }).Build());

            attachedMemberProvider.Register(BindableMembers.For<IViewPager>()
                .SelectedIndexChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberObserver.Add(target, listener, member.Name))
                .Build());
            return configuration;
        }

        #endregion
    }
}