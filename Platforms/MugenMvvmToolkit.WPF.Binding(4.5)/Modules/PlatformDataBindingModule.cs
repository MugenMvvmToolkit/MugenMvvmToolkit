#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MugenMvvmToolkit.WPF.MarkupExtensions;
using MugenMvvmToolkit.WPF.Binding.Converters;
using MugenMvvmToolkit.WPF.Binding.Infrastructure;
using MugenMvvmToolkit.WPF.Binding.Models;
using BooleanToVisibilityConverter = MugenMvvmToolkit.WPF.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.WPF.Binding.Modules
#elif WINDOWS_UWP
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MugenMvvmToolkit.UWP.MarkupExtensions;
using MugenMvvmToolkit.UWP.Binding.Converters;
using MugenMvvmToolkit.UWP.Binding.Infrastructure;
using MugenMvvmToolkit.UWP.Binding.Models;
using BooleanToVisibilityConverter = MugenMvvmToolkit.UWP.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.UWP.Binding.Modules
#endif

{
    public class PlatformDataBindingModule : IModule
    {
        //        #region Constructors
        //
        //        static PlatformDataBindingModule()
        //        {
        //            if (View.BindChanged == null)
        //                View.BindChanged = OnBindChanged;
        //            ViewManager.ViewCleared += OnViewCleared;
        //#if !WINDOWS_UWP
        //            BindingServiceProvider.ValueConverter = BindingReflectionExtensions.Convert;
        //#endif
        //        }
        //
        //        #endregion
        //
        //        #region Methods
        //
        //        private static void OnBindChanged(DependencyObject sender, string bindings)
        //        {
        //            if (string.IsNullOrWhiteSpace(bindings))
        //                return;
        //            if (ServiceProvider.IsDesignMode)
        //            {
        //                IList<IDataBinding> list = BindingServiceProvider.BindingProvider.CreateBindingsFromStringWithBindings(sender, bindings, null);
        //                foreach (InvalidDataBinding binding in list.OfType<InvalidDataBinding>())
        //                    throw binding.Exception;
        //            }
        //            else
        //                BindingServiceProvider.BindingProvider.CreateBindingsFromString(sender, bindings, null);
        //        }
        //
        //
        //
        //        private static void OnViewCleared(IViewManager viewManager, IViewModel viewModel, object arg3, IDataContext arg4)
        //        {
        //            try
        //            {
        //                ClearBindingsRecursively(arg3 as DependencyObject);
        //            }
        //            catch (Exception e)
        //            {
        //                Tracer.Error(e.Flatten());
        //            }
        //        }
        //
        //        private static void ClearBindingsRecursively(DependencyObject item)
        //        {
        //            if (item == null)
        //                return;
        //            var count = VisualTreeHelper.GetChildrenCount(item);
        //            for (int i = 0; i < count; i++)
        //                ClearBindingsRecursively(VisualTreeHelper.GetChild(item, i));
        //            item.ClearBindings(true, true);
        //        }
        //
        //        #endregion
        //
        //        #region Overrides of DataBindingModule
        //
        //        protected override void OnLoaded(IModuleContext context)
        //        {
        //            if (View.BindChanged == null)
        //                View.BindChanged = OnBindChanged;
        //
        //            Register(BindingServiceProvider.MemberProvider);
        //
        //            var resourceResolver = BindingServiceProvider.ResourceResolver;
        //            resourceResolver.AddObject("Visible", new BindingResourceObject(Visibility.Visible), true);
        //            resourceResolver.AddObject("Collapsed", new BindingResourceObject(Visibility.Collapsed), true);
        //
        //            IValueConverter conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed);
        //            resourceResolver
        //                .AddConverter("FalseToCollapsed", new ValueConverterWrapper(conv), true);
        //            conv = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed);
        //            resourceResolver
        //                .AddConverter("TrueToCollapsed", new ValueConverterWrapper(conv), true);
        //
        //            conv = new NullToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);
        //            resourceResolver
        //                .AddConverter("NullToCollapsed", new ValueConverterWrapper(conv), true);
        //            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
        //            resourceResolver
        //                .AddConverter("NotNullToCollapsed", new ValueConverterWrapper(conv), true);
        //
        //#if WPF
        //            conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Hidden, Visibility.Hidden);
        //            resourceResolver
        //                .AddConverter("FalseToHidden", new ValueConverterWrapper(conv), true);
        //            conv = new BooleanToVisibilityConverter(Visibility.Hidden, Visibility.Visible, Visibility.Hidden);
        //            resourceResolver
        //                .AddConverter("TrueToHidden", new ValueConverterWrapper(conv), true);
        //
        //            conv = new NullToVisibilityConverter(Visibility.Hidden, Visibility.Visible);
        //            resourceResolver
        //                .AddConverter("NullToHidden", new ValueConverterWrapper(conv), true);
        //            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Hidden);
        //            resourceResolver
        //                .AddConverter("NotNullToHidden", new ValueConverterWrapper(conv), true);
        //#endif
        //            base.OnLoaded(context);
        //        }
        //
        //        protected override IBindingErrorProvider GetBindingErrorProvider(IModuleContext context)
        //        {
        //#if WPF
        //            if (context.Platform.Platform != PlatformType.WPF)
        //                return null;
        //#endif
        //            return new BindingErrorProvider();
        //        }
        //
        //        protected override IBindingMemberProvider GetBindingMemberProvider(IModuleContext context)
        //        {
        //            var provider = BindingServiceProvider.MemberProvider as BindingMemberProvider;
        //            return provider == null
        //                ? new BindingMemberProviderEx()
        //                : new BindingMemberProviderEx(provider);
        //        }
        //
        //        protected override IBindingContextManager GetBindingContextManager(IModuleContext context)
        //        {
        //            return new BindingContextManagerEx();
        //        }
        //
        //        protected override IBindingResourceResolver GetBindingResourceResolver(IModuleContext context)
        //        {
        //            var resolver = BindingServiceProvider.ResourceResolver as BindingResourceResolver;
        //            return resolver == null
        //                ? new BindingResourceResolverEx()
        //                : new BindingResourceResolverEx(resolver);
        //        }
        //
        //
        //
        //        #endregion

        private static void RegisterType(Type type)
        {
            if (BindingServiceProvider.DisableConverterAutoRegistration)
                return;
            if (!typeof(IValueConverter).IsAssignableFrom(type) || !type.IsPublicNonAbstractClass())
                return;
            var constructor = type.GetConstructor(Empty.Array<Type>());
            if (constructor == null || !constructor.IsPublic)
                return;
            var converter = (IValueConverter)constructor.Invoke(Empty.Array<object>());
            BindingServiceProvider.ResourceResolver.AddConverter(new ValueConverterWrapper(converter), type, true);
            ServiceProvider.BootstrapCodeBuilder?.Append(nameof(BindingExtensions), $"{typeof(BindingExtensions).FullName}.AddConverter(resolver, new {typeof(ValueConverterWrapper).FullName}(new {type.GetPrettyName()}()), typeof({type.GetPrettyName()}), true);");
            if (Tracer.TraceInformation)
                Tracer.Info("The {0} converter is registered.", type);
        }

        #region Properties

        public int Priority => ApplicationSettings.ModulePriorityBinding;

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
            context.TryRegisterDataTemplateSelectorsAndValueConverters(RegisterType);
            MugenMvvmToolkit.Binding.AttachedMembersRegistration.RegisterDefaultMembers();

            AttachedMembersRegistration.RegisterUIElementMembers();
            AttachedMembersRegistration.RegisterFrameworkElementMembers();
            AttachedMembersRegistration.RegisterTextBoxMembers();
            AttachedMembersRegistration.RegisterTextBlockMembers();
            AttachedMembersRegistration.RegisterButtonMembers();
            AttachedMembersRegistration.RegisterComboBoxMembers();
            AttachedMembersRegistration.RegisterListBoxMembers();
            AttachedMembersRegistration.RegisterProgressBarMembers();
#if !WINDOWS_UWP
            AttachedMembersRegistration.RegisterWebBrowserMembers();
#endif
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}
