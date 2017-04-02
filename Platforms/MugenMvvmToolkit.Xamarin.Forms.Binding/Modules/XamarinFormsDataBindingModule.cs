#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsDataBindingModule.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Converters;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Infrastructure;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Modules
{
    public class XamarinFormsDataBindingModule : IModule
    {
        #region Constructors

        static XamarinFormsDataBindingModule()
        {
            BindingServiceProvider.DataContextMemberAliases.Add(nameof(BindableObject.BindingContext));
            BindingServiceProvider.BindingMemberPriorities[nameof(BindableObject.BindingContext)] = BindingServiceProvider.DataContextMemberPriority;
            BindingServiceProvider.CompiledExpressionInvokerSupportCoalesceExpression = false;
        }

        #endregion

        #region Properties

        public int Priority => ApplicationSettings.ModulePriorityInitialization + 1;

        #endregion

        #region Methods

        private static void RegisterType(Type type)
        {
            if (BindingServiceProvider.DisableConverterAutoRegistration)
                return;
            var typeInfo = type.GetTypeInfo();
            if (!typeof(IValueConverter).GetTypeInfo().IsAssignableFrom(typeInfo) || !type.IsPublicNonAbstractClass())
                return;
            var constructor = type.GetConstructor(Empty.Array<Type>());
            if ((constructor == null) || !constructor.IsPublic)
                return;
            var converter = (IValueConverter)constructor.Invoke(Empty.Array<object>());
            BindingServiceProvider.ResourceResolver.AddConverter(new ValueConverterWrapper(converter), type, true);
            ServiceProvider.BootstrapCodeBuilder?.Append(nameof(BindingExtensions),
                $"{typeof(BindingExtensions).FullName}.AddConverter(resolver, new {typeof(ValueConverterWrapper).FullName}(new {type.GetPrettyName()}()), typeof({type.GetPrettyName()}, true);");
            if (Tracer.TraceInformation)
                Tracer.Info("The {0} converter is registered.", type);
        }

        private static void OnAppInitialized(object sender, EventArgs eventArgs)
        {
            ServiceProvider.ViewManager.ViewCleared += OnViewCleared;
        }

        private static void OnViewCleared(IViewManager viewManager, ViewClearedEventArgs args)
        {
            try
            {
                (args.View as BindableObject).ClearBindingsRecursively(true, true);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
            BindingServiceProvider.Initialize(contextManager: new XamarinFormsBindingContextManager(), resourceResolver: new XamarinFormsBindingResourceResolver(),
                errorProvider: new XamarinFormsBindingErrorProvider(), converter: XamarinFormsDataBindingExtensions.Convert);
            context.TryRegisterDataTemplateSelectorsAndValueConverters(RegisterType);
            MugenMvvmToolkit.Binding.AttachedMembersRegistration.RegisterDefaultMembers();

            AttachedMembersRegistration.RegisterElementMembers();
            AttachedMembersRegistration.RegisterVisualElementMembers();
            AttachedMembersRegistration.RegisterToolbarItemMembers();
            AttachedMembersRegistration.RegisterEntryMembers();
            AttachedMembersRegistration.RegisterLabelMembers();
            AttachedMembersRegistration.RegisterButtonMembers();
            AttachedMembersRegistration.RegisterListViewMembers();
            AttachedMembersRegistration.RegisterProgressBarMembers();

            ServiceProvider.Initialized += OnAppInitialized;
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}