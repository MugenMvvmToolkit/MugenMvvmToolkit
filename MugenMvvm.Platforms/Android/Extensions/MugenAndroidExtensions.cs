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
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Native.Views.Support;
using MugenMvvm.Android.Observation;
using MugenMvvm.Android.Presenters;
using MugenMvvm.Android.Requests;
using MugenMvvm.Android.Views;
using MugenMvvm.App.Configuration;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal.Components;
using MugenMvvm.Threading.Components;
using MugenMvvm.Views;
using IViewManager = MugenMvvm.Interfaces.Views.IViewManager;
using View = Android.Views.View;

namespace MugenMvvm.Android.Extensions
{
    public static class MugenAndroidExtensions
    {
        #region Methods

        public static IView GetOrCreateView(this IViewModelBase viewModel, Object container, int resourceId, IReadOnlyMetadataContext? metadata = null, IViewManager? viewManager = null) =>
            viewManager.DefaultIfNull().InitializeAsync(ViewMapping.Undefined, new AndroidViewRequest(viewModel, container, resourceId), default, metadata).Result;

        public static MugenApplicationConfiguration AndroidConfiguration(this MugenApplicationConfiguration configuration, Context? context = null, bool rawViewTagMode = true, bool nativeMode = false,
            bool disableFragmentState = false)
        {
            MugenAndroidNativeService.Initialize(context ?? Application.Context, new AndroidBindViewCallback(), rawViewTagMode);
            LifecycleExtensions.AddLifecycleDispatcher(new AndroidNativeViewLifecycleDispatcher(), nativeMode);

            if (nativeMode)
                MugenAndroidNativeService.SetNativeMode();
            if (disableFragmentState)
                MugenAndroidNativeService.DisableFragmentState();
            configuration.ServiceConfiguration<IThreadDispatcher>()
                .WithComponent(new SynchronizationContextThreadDispatcher(Application.SynchronizationContext));

            configuration.ServiceConfiguration<IAttachedValueManager>()
                .WithComponent(new AndroidAttachedValueStorageProvider());

            configuration.ServiceConfiguration<IViewManager>()
                .WithComponent(new AndroidViewStateDispatcher())
                .WithComponent(new AndroidViewInitializer())
                .WithComponent(new AndroidViewRequestManager())
                .WithComponent(new AndroidViewStateMapperDispatcher())
                .WithComponent(new AndroidActivityViewRequestManager())
                .WithComponent(new AndroidViewMappingDecorator());

            configuration.ServiceConfiguration<IPresenter>()
                .WithComponent(new ActivityViewPresenter())
                .WithComponent(new FragmentDialogViewPresenter());

            configuration
                .ServiceConfiguration<IWeakReferenceManager>()
                .Service()
                .GetOrAddComponent(ctx => new WeakReferenceProviderComponent())
                .TrackResurrection = true;

            return configuration;
        }

        public static MugenApplicationConfiguration WithSupportLibs(this MugenApplicationConfiguration configuration, bool compat, bool material, bool recyclerView, bool swipeRefresh, bool viewPager, bool viewPager2)
        {
            MugenAndroidNativeService.WithSupportLibs(compat, material, recyclerView, swipeRefresh, viewPager, viewPager2);
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
                .PropertyChangedHandler((member, target, value, newValue, metadata) => AndroidMenuItemsSourceGenerator.GetOrAdd(target).Collection = newValue)
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

        public static MugenApplicationConfiguration AndroidAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var attachedMemberProvider = configuration.ServiceConfiguration<IMemberManager>().Service().GetOrAddComponent(context => new AttachedMemberProvider());
            //activity
            attachedMemberProvider.Register(BindableMembers.For<object>()
                .Parent()
                .Override<IActivityView>()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => null)
                .Build());

            //view
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Parent()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => ViewExtensions.GetParent(target))
                .CustomSetter((member, target, value, metadata) => ViewExtensions.SetParent(target, (Object)value!))
                .ObservableHandler((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.ParentMemberName))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Visible()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Visibility == ViewStates.Visible)
                .CustomSetter((member, target, value, metadata) => target.Visibility = value ? ViewStates.Visible : ViewStates.Gone)
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Invisible()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.Visibility == ViewStates.Invisible)
                .CustomSetter((member, target, value, metadata) => target.Visibility = value ? ViewStates.Invisible : ViewStates.Gone)
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .RelativeSourceMethod()
                .RawMethod
                .GetBuilder()
                .WithParameters(AttachedMemberBuilder.Parameter<string>("p1").Build(), AttachedMemberBuilder.Parameter<string>("p2").DefaultValue(BoxingExtensions.Box(1)).Build())
                .InvokeHandler((member, target, args, metadata) => ViewExtensions.FindRelativeSource(target, (string)args[0]!, (int)args[1]!))
                .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .ParentChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.ParentEventName))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Click()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.ClickEventName))
                .Build());

            //textview
            attachedMemberProvider.Register(new BindablePropertyDescriptor<View, string>(AndroidViewMemberChangedListener.TextMemberName)
                .GetBuilder()
                .CustomGetter((member, target, metadata) => TextViewExtensions.GetText(target))
                .CustomSetter((member, target, value, metadata) => TextViewExtensions.SetText(target, value))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .TextChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.TextEventName))
                .Build());

            //swiperefreshlayout
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Refreshed()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.RefreshedEventName))
                .Build());

            //actionbar
            attachedMemberProvider.Register(BindableMembers.For<Object>()
                .ActionBarHomeButtonClick()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.HomeButtonClick))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<Object>()
                .Enabled()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    if (ActionBarExtensions.IsSupported(target))
                        ActionBarExtensions.SetDisplayHomeAsUpEnabled(target, newValue);
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<Object>()
                .ParentNative()
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (ActionBarExtensions.IsSupported(target))
                        return ActivityExtensions.GetActivity(ActionBarExtensions.GetThemedContext(target));
                    return null;
                })
                .Build());

            //toolbar
            attachedMemberProvider.Register(new BindablePropertyDescriptor<View, string>("Title")
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (ToolbarExtensions.IsSupported(target))
                        return ToolbarExtensions.GetTitle(target);
                    BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return null!;
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (ToolbarExtensions.IsSupported(target))
                        ToolbarExtensions.SetTitle(target, value);
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());
            attachedMemberProvider.Register(new BindablePropertyDescriptor<View, string>("Subtitle")
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (ToolbarExtensions.IsSupported(target))
                        return ToolbarExtensions.GetSubtitle(target);
                    BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return null!;
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (ToolbarExtensions.IsSupported(target))
                        ToolbarExtensions.SetSubtitle(target, value);
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .MenuTemplate()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    if (ViewExtensions.IsMenuSupported(target))
                    {
                        var menu = ViewExtensions.GetMenu(target);
                        oldValue?.Clear(menu);
                        newValue?.Apply(menu, target);
                    }
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());


            //viewgroup
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .ContentTemplateSelector()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    if (newValue is IResourceTemplateSelector selector)
                        member.SetValue(target, new ContentTemplateSelectorWrapper(selector), metadata);
                })
                .NonObservable()
                .DefaultValue(DefaultContentTemplateSelector.Instance)
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Content()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => ViewGroupExtensions.GetContent(target)?.BindableMembers().DataContext())
                .CustomSetter((member, target, value, metadata) =>
                {
                    var contentTemplateSelector = target.BindableMembers().ContentTemplateSelector();
                    if (contentTemplateSelector == null)
                        ExceptionManager.ThrowNotSupported(nameof(contentTemplateSelector));

                    var newValue = (Object)contentTemplateSelector.SelectTemplate(target, value)!;
                    if (newValue != null)
                    {
                        newValue.BindableMembers().SetDataContext(value);
                        newValue.BindableMembers().SetParent(target);
                    }

                    ViewGroupExtensions.SetContent(target, newValue!);
                })
                .Observable()
                .Build());

            //tablayout.tab
            attachedMemberProvider.Register(new BindablePropertyDescriptor<Object, string>(AndroidViewMemberChangedListener.TextMemberName)
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (TabLayoutTabExtensions.IsSupported(target))
                        return TabLayoutTabExtensions.GetText(target);
                    BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return null!;
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (TabLayoutTabExtensions.IsSupported(target))
                    {
                        TabLayoutTabExtensions.SetText(target, value);
                        return;
                    }

                    BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());

            //adapterview/recyclerview/viewpager/viewpager2/viewgroup/tablayout
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .StableIdProvider()
                .GetBuilder()
                .NonObservable()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .ItemTemplateSelector()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    if (!(newValue is IResourceTemplateSelector selector))
                        return;

                    var providerType = ViewGroupExtensions.GetItemSourceProviderType(target);
                    if (providerType == ViewGroupExtensions.ContentProviderType || providerType == ViewGroupExtensions.ContentRawProviderType
                                                                                || (providerType == ViewGroupExtensions.ResourceOrContentProviderType && selector is IFragmentTemplateSelector fts && fts.HasFragments))
                        member.SetValue(target, new ContentTemplateSelectorWrapper(selector), metadata);
                })
                .NonObservable()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .ItemsSource()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    var providerType = ViewGroupExtensions.GetItemSourceProviderType(target);
                    if (providerType == ViewGroupExtensions.NoneProviderType)
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    var itemTemplateSelector = target.BindableMembers().ItemTemplateSelector();
                    if (itemTemplateSelector == null)
                        ExceptionManager.ThrowObjectNotInitialized(target, target.BindableMembers().Descriptor.ItemTemplateSelector());

                    var itemsSourceProvider = ViewGroupExtensions.GetItemsSourceProvider(target);
                    bool hasFragments = itemTemplateSelector is IFragmentTemplateSelector fts && fts.HasFragments;
                    if (providerType == ViewGroupExtensions.ContentRawProviderType)
                        AndroidContentItemsSourceGenerator.GetOrAdd(target, (IContentTemplateSelector)itemTemplateSelector).Collection = newValue;
                    else if (providerType == ViewGroupExtensions.ContentProviderType || (providerType == ViewGroupExtensions.ResourceOrContentProviderType && hasFragments))
                    {
                        if (!(itemsSourceProvider is AndroidContentItemsSourceProvider provider))
                        {
                            ViewExtensions.RemoveParentObserver(target);
                            provider = new AndroidContentItemsSourceProvider(target, (IContentTemplateSelector)itemTemplateSelector, target.BindableMembers().StableIdProvider());
                            ViewGroupExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                        }

                        provider.ItemsSource = newValue;
                    }
                    else
                    {
                        if (!(itemsSourceProvider is AndroidResourceItemsSourceProvider provider))
                        {
                            ViewExtensions.RemoveParentObserver(target);
                            provider = new AndroidResourceItemsSourceProvider(target, (IResourceTemplateSelector)itemTemplateSelector, target.BindableMembers().StableIdProvider());
                            ViewGroupExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                        }

                        provider.ItemsSource = newValue;
                    }
                }).Build());

            //viewpager/viewpager2/tablayout
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedIndex()
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (!ViewGroupExtensions.IsSelectedIndexSupported(target))
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return ViewGroupExtensions.GetSelectedIndex(target);
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (!ViewGroupExtensions.SetSelectedIndex(target, value))
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedItem()
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (!ViewGroupExtensions.IsSelectedIndexSupported(target))
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    var index = ViewGroupExtensions.GetSelectedIndex(target);
                    if (index < 0)
                        return null;
                    var itemsSourceProvider = GetItemsSourceProvider(target);
                    if (itemsSourceProvider == null)
                        return null;
                    if (itemsSourceProvider is IAndroidItemsSourceProvider p)
                        return p.GetItemAt(index);
                    return ((BindableCollectionAdapter)itemsSourceProvider)[index];
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    int index;
                    var itemsSourceProvider = GetItemsSourceProvider(target);
                    if (itemsSourceProvider == null)
                        index = -1;
                    else
                    {
                        if (itemsSourceProvider is IAndroidItemsSourceProvider p)
                            index = p.IndexOf(value);
                        else
                            index = ((BindableCollectionAdapter)itemsSourceProvider).IndexOf(value);
                    }

                    if (!ViewGroupExtensions.SetSelectedIndex(target, index))
                        BindingExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedIndexChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.SelectedIndexEventName))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedItemChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => AndroidViewMemberChangedListener.Add(target, listener, AndroidViewMemberChangedListener.SelectedIndexEventName))
                .Build());
            return configuration;
        }

        private static object? GetItemsSourceProvider(View target)
        {
            var itemsSourceProvider = ViewGroupExtensions.GetItemsSourceProvider(target);
            if (itemsSourceProvider != null)
                return itemsSourceProvider;
            return AndroidContentItemsSourceGenerator.Get(target);
        }

        #endregion
    }
}