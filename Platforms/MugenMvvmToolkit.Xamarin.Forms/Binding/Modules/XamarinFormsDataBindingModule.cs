#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsDataBindingModule.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Converters;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Modules
{
    public class XamarinFormsDataBindingModule : IModule
    {
        #region Properties

        //        #region Overrides of DataBindingModule

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
        //        #endregion
        public int Priority => ApplicationSettings.ModulePriorityBinding + 1;

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

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
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
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}