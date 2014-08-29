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
using System.Diagnostics;
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

        private static readonly HashSet<Type> ImplicitParentTypes;
        private static readonly bool DebbugerAttached;
        private static readonly IAttachedBindingMemberInfo<object, object> CommandParameterInternal;

        #endregion

        #region Constructors

        static DataBindingModule()
        {
            DebbugerAttached = Debugger.IsAttached;
            ImplicitParentTypes = new HashSet<Type>();
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
            var assemblies = context.Assemblies;
            for (int i = 0; i < assemblies.Count; i++)
            {
                var types = assemblies[i].SafeGetTypes(context.Mode != LoadMode.Design);
                for (int j = 0; j < types.Count; j++)
                    RegisterType(types[j]);
            }


            BindingServiceProvider.MemberProvider.Register(
                AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.CommandParameter,
                    GetCommandParameter, SetCommandParameter, ObserveCommandParameter));
            BindingServiceProvider.MemberProvider.Register(
                AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.Parent,
                    GetParent, SetParent, ObserveParent));

            BindingServiceProvider.MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IEnumerable<object>>(
                    AttachedMemberConstants.ErrorsPropertyMember, getDefaultValue: (o, info) => Enumerable.Empty<object>()));
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
        /// Tries to register type.
        /// </summary>
        protected virtual void RegisterType(Type type)
        {
#if PCL_WINRT
            if (typeof(IBindingValueConverter).IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract &&
                type.GetTypeInfo().IsClass)

#else
            if (typeof(IBindingValueConverter).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)
#endif
            {
                var constructor = type.GetConstructor(EmptyValue<Type>.ArrayInstance);
                if (constructor == null || !constructor.IsPublic)
                    return;
                var converter = (IBindingValueConverter)constructor.InvokeEx();
                string name = RemoveTail(RemoveTail(RemoveTail(type.Name, "BindingValueConverter"), "ValueConverter"), "Converter");
                if (BindingServiceProvider.ResourceResolver.TryAddConverter(name, converter))
                    Tracer.Info("The {0} converter is registered.", type);
            }
        }

        /// <summary>
        ///     Removes the tail
        /// </summary>
        protected static string RemoveTail(string name, string word)
        {
            if (name.EndsWith(word, StringComparison.OrdinalIgnoreCase))
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

        private static IDisposable ObserveParent(IBindingMemberInfo bindingMemberInfo, object o, IEventListener arg3)
        {
            return GetParentMember(o).TryObserve(o, arg3);
        }

        private static object SetParent(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            return GetParentMember(o).SetValue(o, arg3);
        }

        private static object GetParent(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            var member = GetParentMember(o);
            var value = member.GetValue(o, arg3);
            if (DebbugerAttached && value == null)
            {
                lock (ImplicitParentTypes)
                {
                    var type = o.GetType();
                    if (!ImplicitParentTypes.Contains(type))
                    {
                        Tracer.Warn(@"Could not find a 'Parent' property on type '{0}', you should register it, without this the data bindings may not work properly. You can ignore this message if you are using the BindingExtensions.AttachedParentMember property.", type);
                        ImplicitParentTypes.Add(type);
                    }
                }
            }
            return value;
        }

        private static IBindingMemberInfo GetParentMember(object instance)
        {
            return BindingServiceProvider
                .MemberProvider
                .GetBindingMember(instance.GetType(), AttachedMemberConstants.Parent, true, false) ??
                   BindingExtensions.AttachedParentMember;
        }

        #endregion
    }
}