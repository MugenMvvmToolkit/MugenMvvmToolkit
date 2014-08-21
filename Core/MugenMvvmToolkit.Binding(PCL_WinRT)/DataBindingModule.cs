#region Copyright
// ****************************************************************************
// <copyright file="DataBindingModule.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Reflection;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding
{
    /// <summary>
    ///     Represents the binding module.
    /// </summary>
    public class DataBindingModule : IModule
    {
        #region Fields

        private static readonly IAttachedBindingMemberInfo<object, object> CommandParameterInternal;

        #endregion

        #region Constructors

        static DataBindingModule()
        {
            CommandParameterInternal = AttachedBindingMember.CreateAutoProperty<object, object>("~#@cmdparam");
        }

        #endregion

        #region Implementation of IModule

        /// <summary>
        ///     Gets the priority.
        /// </summary>
        public virtual int Priority
        {
            get { return ModuleBase.BindingModulePriority; }
        }

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        public virtual bool Load(IModuleContext context)
        {
            Should.NotBeNull(context, "context");
            IEnumerable<Type> converterTypes = context
                .Assemblies
                .SelectMany(assembly => assembly.SafeGetTypes(context.Mode != LoadMode.Design))
#if PCL_WINRT
.Where(type => typeof(IBindingValueConverter).IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract && type.GetTypeInfo().IsClass);
#else
.Where(type => typeof(IBindingValueConverter).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass);
#endif


            foreach (Type type in converterTypes)
            {
                var constructor = type.GetConstructor(EmptyValue<Type>.ArrayInstance);
                if (constructor == null || !constructor.IsPublic)
                    continue;
                var converter = (IBindingValueConverter)constructor.InvokeEx();
                string name = RemoveTail(RemoveTail(type.Name, "ValueConverter"), "Converter");
                if (BindingServiceProvider.ResourceResolver.TryAddConverter(name, converter))
                    Tracer.Info("The {0} converter is registered.", type);
            }
            BindingServiceProvider.MemberProvider.Register(
                AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.CommandParameter,
                    GetCommandParameter, SetCommandParameter, ObserveCommandParameter));
            BindingServiceProvider.MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IEnumerable<object>>(
                    AttachedMemberConstants.ErrorsPropertyMember, defaultValue: (o, info) => Enumerable.Empty<object>()));
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        public virtual void Unload(IModuleContext context)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Removes the tail
        /// </summary>
        protected static string RemoveTail(string name, string word)
        {
            if (name.EndsWith(word))
                name = name.Substring(0, name.Length - word.Length);
            return name;
        }

        private static IDisposable ObserveCommandParameter(IBindingMemberInfo bindingMemberInfo, object o, IEventListener arg3)
        {
            return GetCommandParameterMember(o).TryObserve(o, arg3);
        }

        private static object SetCommandParameter(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            return GetCommandParameterMember(o).SetValue(o, arg3);
        }

        private static object GetCommandParameter(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            return GetCommandParameterMember(o).GetValue(o, arg3);
        }

        private static IBindingMemberInfo GetCommandParameterMember(object instance)
        {
            return BindingServiceProvider
                 .MemberProvider
                 .GetBindingMember(instance.GetType(), AttachedMemberConstants.CommandParameter, true, false) ?? CommandParameterInternal;
        }

        #endregion
    }
}