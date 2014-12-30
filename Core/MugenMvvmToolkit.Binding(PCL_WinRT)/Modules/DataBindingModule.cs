#region Copyright

// ****************************************************************************
// <copyright file="DataBindingModule.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;

namespace MugenMvvmToolkit.Binding.Modules
{
    /// <summary>
    ///     Represents the binding module.
    /// </summary>
    public class DataBindingModule : IModule
    {
        #region Fields

        private static bool _isLoaded;
        private const string ErrorProviderErrors = "SetBindingErrors";
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
        public bool Load(IModuleContext context)
        {
            Should.NotBeNull(context, "context");
            if (!CanLoad(context))
                return false;

            InitilaizeServices();

            if (!_isLoaded || GetType().GetMethodEx("RegisterType", MemberFlags.Instance | MemberFlags.NonPublic).IsOverride(typeof(DataBindingModule)))
            {
                var assemblies = context.Assemblies;
                for (int i = 0; i < assemblies.Count; i++)
                {
                    var types = assemblies[i].SafeGetTypes(context.Mode != LoadMode.Design);
                    for (int j = 0; j < types.Count; j++)
                        RegisterType(types[j]);
                }
            }

            if (!_isLoaded)
            {
                var memberProvider = BindingServiceProvider.MemberProvider;
                memberProvider
                    .Register(AttachedBindingMember
                        .CreateMember<object, object>(AttachedMemberConstants.CommandParameter, GetCommandParameter,
                            SetCommandParameter, ObserveCommandParameter));
                memberProvider
                    .Register(AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.Parent,
                        GetParent, SetParent, ObserveParent));
                memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IEnumerable<object>>(
                        AttachedMemberConstants.ErrorsPropertyMember, getDefaultValue: (o, info) => Empty.Array<object>()));
                memberProvider.Register(AttachedBindingMember.CreateMember<object, bool>("HasErrors", GetHasErrors, null, ObserveHasErrors));
                var setErrorsMember = AttachedBindingMember.CreateMember<object, IEnumerable<object>>(ErrorProviderErrors, getValue: null,
                    setValue: SetErrorProviderErrors);
                memberProvider.Register(setErrorsMember);
                memberProvider.Register(typeof(object), "BindingErrorProvider.Errors", setErrorsMember, true);
                _isLoaded = true;
            }

            OnLoaded(context);
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        public void Unload(IModuleContext context)
        {
            OnUnloaded(context);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Checks to see whether the module can be loaded with specified context.
        /// </summary>
        protected virtual bool CanLoad(IModuleContext context)
        {
            return true;
        }

        /// <summary>
        ///    Occurs on load the current module.
        /// </summary>
        protected virtual void OnLoaded(IModuleContext context)
        {
        }

        /// <summary>
        ///     Occurs on unload the current module.
        /// </summary>
        protected virtual void OnUnloaded(IModuleContext context)
        {
        }

        /// <summary>
        ///     Tries to register type.
        /// </summary>
        protected virtual void RegisterType(Type type)
        {
            if (!typeof(IBindingValueConverter).IsAssignableFrom(type) || !type.IsPublicNonAbstractClass())
                return;
            var constructor = type.GetConstructor(Empty.Array<Type>());
            if (constructor == null || !constructor.IsPublic)
                return;
            var converter = (IBindingValueConverter)constructor.InvokeEx();
            string name = RemoveTail(RemoveTail(RemoveTail(type.Name, "BindingValueConverter"), "ValueConverter"), "Converter");
            if (BindingServiceProvider.ResourceResolver.TryAddConverter(name, converter))
                Tracer.Info("The {0} converter is registered.", type);
        }

        /// <summary>
        ///     Gets the <see cref="IBindingErrorProvider" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IBindingErrorProvider GetBindingErrorProvider()
        {
            return new BindingErrorProviderBase();
        }

        /// <summary>
        ///     Gets the <see cref="IBindingProvider" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IBindingProvider GetBindingProvider()
        {
            return null;
        }

        /// <summary>
        ///     Gets the <see cref="IBindingManager" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IBindingManager GetBindingManager()
        {
            return null;
        }

        /// <summary>
        ///     Gets the <see cref="IBindingMemberProvider" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IBindingMemberProvider GetBindingMemberProvider()
        {
            return null;
        }

        /// <summary>
        ///     Gets the <see cref="IObserverProvider" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IObserverProvider GetObserverProvider()
        {
            return null;
        }

        /// <summary>
        ///     Gets the <see cref="IBindingContextManager" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IBindingContextManager GetBindingContextManager()
        {
            return null;
        }

        /// <summary>
        ///     Gets the <see cref="IBindingResourceResolver" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IBindingResourceResolver GetBindingResourceResolver()
        {
            return null;
        }

        /// <summary>
        ///     Gets the <see cref="IVisualTreeManager" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IVisualTreeManager GetVisualTreeManager()
        {
            return null;
        }

        /// <summary>
        ///     Gets the <see cref="IWeakEventManager" /> that will be used by default.
        /// </summary>
        [CanBeNull]
        protected virtual IWeakEventManager GetWeakEventManager()
        {
            return null;
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

        private void InitilaizeServices()
        {
            var bindingProvider = GetBindingProvider();
            if (bindingProvider != null)
                BindingServiceProvider.BindingProvider = bindingProvider;

            var bindingManager = GetBindingManager();
            if (bindingManager != null)
                BindingServiceProvider.BindingManager = bindingManager;

            var memberProvider = GetBindingMemberProvider();
            if (memberProvider != null)
                BindingServiceProvider.MemberProvider = memberProvider;

            var observerProvider = GetObserverProvider();
            if (observerProvider != null)
                BindingServiceProvider.ObserverProvider = observerProvider;

            var contextManager = GetBindingContextManager();
            if (contextManager != null)
                BindingServiceProvider.ContextManager = contextManager;

            var resourceResolver = GetBindingResourceResolver();
            if (resourceResolver != null)
                BindingServiceProvider.ResourceResolver = resourceResolver;

            var visualTreeManager = GetVisualTreeManager();
            if (visualTreeManager != null)
                BindingServiceProvider.VisualTreeManager = visualTreeManager;

            var weakEventManager = GetWeakEventManager();
            if (weakEventManager != null)
                BindingServiceProvider.WeakEventManager = weakEventManager;

            var errorProvider = GetBindingErrorProvider();
            if (errorProvider != null)
                BindingServiceProvider.ErrorProvider = errorProvider;
        }

        private static bool GetHasErrors(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            var member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(o.GetType(), AttachedMemberConstants.ErrorsPropertyMember, false, false);
            if (member == null)
                return false;
            var value = member.GetValue(o, arg3) as ICollection<object>;
            return value != null && value.Count != 0;
        }

        private static IDisposable ObserveHasErrors(IBindingMemberInfo bindingMemberInfo, object o, IEventListener arg3)
        {
            var member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(o.GetType(), AttachedMemberConstants.ErrorsPropertyMember, false, false);
            if (member == null)
                return null;
            return member.TryObserve(o, arg3);
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

        private static void SetErrorProviderErrors(IBindingMemberInfo bindingMemberInfo, object o, IEnumerable<object> errors)
        {
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return;
            var errorsList = errors as IList<object>;
            if (errorsList == null)
                errorsList = errors == null ? Empty.Array<object>() : errors.ToArray();
            errorProvider.SetErrors(o, ErrorProviderErrors, errorsList, DataContext.Empty);
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