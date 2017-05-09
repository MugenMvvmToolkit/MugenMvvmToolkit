#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
#if WPF
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MugenMvvmToolkit.WPF.MarkupExtensions;
using MugenMvvmToolkit.WPF.Binding.Converters;
using MugenMvvmToolkit.WPF.Binding.Infrastructure;

namespace MugenMvvmToolkit.WPF.Binding.Modules
{


    public class WpfDataBindingModule : IModule
#elif WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using MugenMvvmToolkit.UWP.MarkupExtensions;
using MugenMvvmToolkit.UWP.Binding.Converters;
using MugenMvvmToolkit.UWP.Binding.Infrastructure;
using BooleanToVisibilityConverter = MugenMvvmToolkit.UWP.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.UWP.Binding.Modules
{

    public class UwpDataBindingModule : IModule
#endif

    {
        #region Constructors

#if WPF
        static WpfDataBindingModule()
#elif WINDOWS_UWP
        static UwpDataBindingModule()
#endif
        {
            if (View.BindChanged == null)
                View.BindChanged = OnBindChanged;
        }

        #endregion

        #region Methods

        private static void OnBindChanged(DependencyObject sender, string bindings)
        {
            if (string.IsNullOrWhiteSpace(bindings))
                return;
            if (ServiceProvider.IsDesignMode)
            {
#if WINDOWS_UWP
                UwpDataBindingExtensions.InitializeFromDesignContext();
#else
                WpfDataBindingExtensions.InitializeFromDesignContext();
#endif
                IList<IDataBinding> list = BindingServiceProvider.BindingProvider.CreateBindingsFromStringWithBindings(sender, bindings, null);
                foreach (InvalidDataBinding binding in list.OfType<InvalidDataBinding>())
                    throw binding.Exception;
            }
            else
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(sender, bindings, null);
        }

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

        private static void OnViewCleared(IViewManager sender, ViewClearedEventArgs args)
        {
            try
            {
                ClearBindingsRecursively(args.View as DependencyObject);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten());
            }
        }

        private static void ClearBindingsRecursively(DependencyObject item)
        {
            if (item == null)
                return;
            var count = VisualTreeHelper.GetChildrenCount(item);
            for (int i = 0; i < count; i++)
                ClearBindingsRecursively(VisualTreeHelper.GetChild(item, i));
            item.ClearBindings(true, true);
        }

        #endregion

        #region Properties

        public int Priority => ApplicationSettings.ModulePriorityInitialization - 1;

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
#if WPF
            if (context.PlatformInfo.Platform == PlatformType.WPF)
            {
                BindingServiceProvider.Initialize(memberProvider: new WpfBindingMemberProvider(), contextManager: new WpfBindingContextManager(),
                    errorProvider: new WpfBindingErrorProvider(), resourceResolver: new WpfBindingResourceResolver(), converter: BindingConverterExtensions.Convert);
            }
#elif WINDOWS_UWP
            if (context.PlatformInfo.Platform == PlatformType.UWP)
            {
                BindingServiceProvider.Initialize(memberProvider: new UwpBindingMemberProvider(), contextManager: new UwpBindingContextManager(),
                    errorProvider: new UwpBindingErrorProvider(), resourceResolver: new UwpBindingResourceResolver(), converter: BindingConverterExtensions.Convert);
            }
#endif
            context.TryRegisterDataTemplateSelectorsAndValueConverters(RegisterType);
            var resourceResolver = BindingServiceProvider.ResourceResolver;
            resourceResolver.AddObject("Visible", new BindingResourceObject(Visibility.Visible), true);
            resourceResolver.AddObject("Collapsed", new BindingResourceObject(Visibility.Collapsed), true);

            IValueConverter conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed);
            resourceResolver.AddConverter("FalseToCollapsed", new ValueConverterWrapper(conv), true);
            conv = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed);
            resourceResolver.AddConverter("TrueToCollapsed", new ValueConverterWrapper(conv), true);

            conv = new NullToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);
            resourceResolver.AddConverter("NullToCollapsed", new ValueConverterWrapper(conv), true);
            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
            resourceResolver.AddConverter("NotNullToCollapsed", new ValueConverterWrapper(conv), true);

#if WPF
            conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Hidden, Visibility.Hidden);
            resourceResolver.AddConverter("FalseToHidden", new ValueConverterWrapper(conv), true);
            conv = new BooleanToVisibilityConverter(Visibility.Hidden, Visibility.Visible, Visibility.Hidden);
            resourceResolver.AddConverter("TrueToHidden", new ValueConverterWrapper(conv), true);

            conv = new NullToVisibilityConverter(Visibility.Hidden, Visibility.Visible);
            resourceResolver.AddConverter("NullToHidden", new ValueConverterWrapper(conv), true);
            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Hidden);
            resourceResolver.AddConverter("NotNullToHidden", new ValueConverterWrapper(conv), true);
#endif
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
            IViewManager viewManager;
            context.IocContainer.TryGet(out viewManager);
            if (viewManager != null)
                viewManager.ViewCleared += OnViewCleared;
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}
