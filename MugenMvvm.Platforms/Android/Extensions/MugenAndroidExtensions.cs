using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Views;
using MugenMvvm.Android.Bindings;
using MugenMvvm.Android.Collections;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Internal;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Native.Views.Support;
using MugenMvvm.Android.Navigation;
using MugenMvvm.Android.Observation;
using MugenMvvm.Android.Presenters;
using MugenMvvm.Android.Requests;
using MugenMvvm.Android.Views;
using MugenMvvm.App.Configuration;
using MugenMvvm.Bindings;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal.Components;
using MugenMvvm.Threading.Components;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using IViewManager = MugenMvvm.Interfaces.Views.IViewManager;
using Object = Java.Lang.Object;
using View = Android.Views.View;

namespace MugenMvvm.Android.Extensions
{
    public static class MugenAndroidExtensions
    {
        #region Fields

        private static float _density = float.MinValue;
        private static float _scaledDensity = float.MinValue;
        private static float _xdpi = float.MinValue;

        #endregion

        #region Methods

        public static IView GetOrCreateView(this IViewModelBase viewModel, Object container, int resourceId, IReadOnlyMetadataContext? metadata = null, IViewManager? viewManager = null) =>
            viewManager.DefaultIfNull().InitializeAsync(ViewMapping.Undefined, new ResourceViewRequest(viewModel, container, resourceId), default, metadata).Result;

        public static MugenApplicationConfiguration AndroidConfiguration(this MugenApplicationConfiguration configuration, Context? context = null,
            bool rawViewTagMode = true, bool nativeMode = false, bool disableFragmentState = false)
        {
            MugenAndroidNativeService.Initialize(context ?? Application.Context, new BindViewCallback(), rawViewTagMode);
            LifecycleExtensions.AddLifecycleDispatcher(new NativeViewLifecycleDispatcher(), nativeMode);

            if (nativeMode)
                MugenAndroidNativeService.SetNativeMode();
            if (disableFragmentState)
                MugenAndroidNativeService.DisableFragmentState();
            configuration.ServiceConfiguration<IThreadDispatcher>()
                .WithComponent(new SynchronizationContextThreadDispatcher(Application.SynchronizationContext));

            configuration.ServiceConfiguration<IAttachedValueManager>()
                .WithComponent(new AndroidAttachedValueStorageProvider());

            configuration.ServiceConfiguration<INavigationDispatcher>()
                .WithComponent(new ViewNavigationConditionDispatcher());

            configuration.ServiceConfiguration<IViewManager>()
                .WithComponent(new ViewStateDispatcher())
                .WithComponent(new ViewLifecycleDispatcher())
                .WithComponent(new ResourceViewRequestManager())
                .WithComponent(new ViewStateMapperDispatcher())
                .WithComponent(new ActivityViewRequestManager())
                .WithComponent(new ResourceViewMappingDecorator())
                .Service()
                .GetOrAddComponent(ctx => new ViewLifecycleTracker())
                .Trackers.Add(TrackViewState);

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

        public static MugenApplicationConfiguration AndroidBindingConfiguration(this MugenApplicationConfiguration configuration)
        {
            var digitTokenParser = configuration
                .ServiceConfiguration<IExpressionParser>()
                .Service()
                .GetOrAddComponent(context => new DigitTokenParser());
            var converter = new DigitTokenParser.ConvertDelegate(ConvertAndroidDigits);
            digitTokenParser.PostfixToConverter[AndroidInternalConstant.DpMetric] = converter;
            digitTokenParser.PostfixToConverter[AndroidInternalConstant.DipMetric] = converter;
            digitTokenParser.PostfixToConverter[AndroidInternalConstant.MmMetric] = converter;
            digitTokenParser.PostfixToConverter[AndroidInternalConstant.InMetric] = converter;
            digitTokenParser.PostfixToConverter[AndroidInternalConstant.PtMetric] = converter;
            digitTokenParser.PostfixToConverter[AndroidInternalConstant.SpMetric] = converter;

            var macrosBindingInitializer = configuration
                .ServiceConfiguration<IBindingManager>()
                .Service()
                .GetOrAddComponent(context => new MacrosBindingInitializer());
            var resourceVisitor = new ResourceExpressionVisitor();
            macrosBindingInitializer.TargetVisitors.Add(resourceVisitor);
            macrosBindingInitializer.SourceVisitors.Add(resourceVisitor);
            macrosBindingInitializer.ParameterVisitors.Add(resourceVisitor);

            return configuration;
        }

        public static MugenApplicationConfiguration WithSupportLibs(this MugenApplicationConfiguration configuration, bool compat, bool material, bool recyclerView, bool swipeRefresh, bool viewPager, bool viewPager2)
        {
            MugenAndroidNativeService.WithSupportLibs(compat, material, recyclerView, swipeRefresh, viewPager, viewPager2);
            return configuration;
        }

        public static MugenApplicationConfiguration AndroidAttachedMembersConfiguration(this MugenApplicationConfiguration configuration)
        {
            var attachedMemberProvider = configuration.ServiceConfiguration<IMemberManager>().Service().GetOrAddComponent(context => new AttachedMemberProvider());
            //object
            attachedMemberProvider.Register(BindableMembers.For<Object>()
                .CollectionViewManager()
                .GetBuilder()
                .DefaultValue((info, view) => new CollectionViewManager())
                .NonObservable()
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .ItemsSource()
                .Override<Object>()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.BindableMembers().CollectionViewManager()?.GetItemsSource(target))
                .CustomSetter((member, target, value, metadata) => target.BindableMembers().CollectionViewManager()?.SetItemsSource(target, value))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedItem()
                .Override<Object>()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => target.BindableMembers().CollectionViewManager()?.GetSelectedItem(target))
                .CustomSetter((member, target, value, metadata) => target.BindableMembers().CollectionViewManager()?.SetSelectedItem(target, value))
                .Build());

            //activity
            attachedMemberProvider.Register(BindableMembers.For<object>()
                .Parent()
                .Override<IActivityView>()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => null)
                .Build());

            //IMenu
            attachedMemberProvider.Register(BindableMembers.For<IMenu>()
                .ItemTemplate()
                .GetBuilder()
                .Build());

            //IMenuItem
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

            //view
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Parent()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => ViewExtensions.GetParent(target))
                .CustomSetter((member, target, value, metadata) => ViewExtensions.SetParent(target, (Object) value!))
                .ObservableHandler((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.ParentMemberName))
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
                .InvokeHandler((member, target, args, metadata) => ViewExtensions.FindRelativeSource(target, (string) args[0]!, (int) args[1]!))
                .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .ParentChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.ParentEventName))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Click()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.ClickEventName))
                .Build());

            //textview
            attachedMemberProvider.Register(new BindablePropertyDescriptor<View, string>(ViewMemberChangedListener.TextMemberName)
                .GetBuilder()
                .CustomGetter((member, target, metadata) => TextViewExtensions.GetText(target))
                .CustomSetter((member, target, value, metadata) => TextViewExtensions.SetText(target, value))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .TextChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.TextEventName))
                .Build());

            //swiperefreshlayout
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .Refreshed()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.RefreshedEventName))
                .Build());

            //actionbar
            attachedMemberProvider.Register(BindableMembers.For<Object>()
                .ActionBarHomeButtonClick()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.HomeButtonClick))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<Object>()
                .Enabled()
                .GetBuilder()
                .PropertyChangedHandler((member, target, oldValue, newValue, metadata) =>
                {
                    if (ActionBarExtensions.IsSupported(target))
                        ActionBarExtensions.SetDisplayHomeAsUpEnabled(target, newValue);
                    else
                        ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
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
                    ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return null!;
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (ToolbarExtensions.IsSupported(target))
                        ToolbarExtensions.SetTitle(target, value);
                    else
                        ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());
            attachedMemberProvider.Register(new BindablePropertyDescriptor<View, string>("Subtitle")
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (ToolbarExtensions.IsSupported(target))
                        return ToolbarExtensions.GetSubtitle(target);
                    ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return null!;
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (ToolbarExtensions.IsSupported(target))
                        ToolbarExtensions.SetSubtitle(target, value);
                    else
                        ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
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
                        ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
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

                    var newValue = (Object?) contentTemplateSelector.SelectTemplate(target, value)!;
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
            attachedMemberProvider.Register(new BindablePropertyDescriptor<Object, string>(ViewMemberChangedListener.TextMemberName)
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (TabLayoutTabExtensions.IsSupported(target))
                        return TabLayoutTabExtensions.GetText(target);
                    ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return null!;
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (TabLayoutTabExtensions.IsSupported(target))
                    {
                        TabLayoutTabExtensions.SetText(target, value);
                        return;
                    }

                    ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
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
                                                                                || providerType == ViewGroupExtensions.ResourceOrContentProviderType && selector is IFragmentTemplateSelector fts && fts.HasFragments)
                        member.SetValue(target, new ContentTemplateSelectorWrapper(selector), metadata);
                })
                .NonObservable()
                .Build());

            //viewpager/viewpager2/tablayout
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedIndex()
                .GetBuilder()
                .CustomGetter((member, target, metadata) =>
                {
                    if (!ViewGroupExtensions.IsSelectedIndexSupported(target))
                        ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                    return ViewGroupExtensions.GetSelectedIndex(target);
                })
                .CustomSetter((member, target, value, metadata) =>
                {
                    if (!ViewGroupExtensions.SetSelectedIndex(target, value))
                        ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                })
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedIndexChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.SelectedIndexEventName))
                .Build());
            attachedMemberProvider.Register(BindableMembers.For<View>()
                .SelectedItemChanged()
                .GetBuilder()
                .CustomImplementation((member, target, listener, metadata) => ViewMemberChangedListener.Add(target, listener, ViewMemberChangedListener.SelectedIndexEventName))
                .Build());
            return configuration;
        }

        private static void TrackViewState(object view, HashSet<ViewLifecycleState> states, ViewLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            if (state == AndroidViewLifecycleState.PendingInitialization)
                states.Add(AndroidViewLifecycleState.PendingInitialization);
        }

        private static IExpressionNode? ConvertAndroidDigits(ReadOnlySpan<char> value, bool integer, string postfix, ITokenParserContext context, IFormatProvider formatProvider)
        {
            if (!float.TryParse(value, NumberStyles.Any, formatProvider, out var floatValue))
                return null;
            switch (postfix)
            {
                case AndroidInternalConstant.DpMetric:
                case AndroidInternalConstant.DipMetric:
                    floatValue *= GetDensity();
                    break;
                case AndroidInternalConstant.SpMetric:
                    floatValue *= GetScaledDensity();
                    break;
                case AndroidInternalConstant.PtMetric:
                    floatValue = floatValue * GetXdpi() * (1.0f / 72);
                    break;
                case AndroidInternalConstant.InMetric:
                    floatValue *= GetXdpi();
                    break;
                case AndroidInternalConstant.MmMetric:
                    floatValue = floatValue * GetXdpi() * (1.0f / 25.4f);
                    break;
            }

            if (integer)
                return ConstantExpressionNode.Get((int) floatValue);
            return ConstantExpressionNode.Get(floatValue);
        }

        private static float GetDensity()
        {
            if (_density == float.MinValue)
                _density = MugenNativeUtils.Density;
            return _density;
        }

        private static float GetScaledDensity()
        {
            if (_scaledDensity == float.MinValue)
                _scaledDensity = MugenNativeUtils.ScaledDensity;
            return _scaledDensity;
        }

        private static float GetXdpi()
        {
            if (_xdpi == float.MinValue)
                _xdpi = MugenNativeUtils.Xdpi;
            return _xdpi;
        }

        #endregion
    }
}