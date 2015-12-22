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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;

namespace MugenMvvmToolkit.Binding.Modules
{
    public class DataBindingModule : IModule
    {
        #region Nested types

        private struct ParentValue
        {
            #region Fields

            private object _attachedParent;
            private IBindingMemberInfo _parentMember;

            #endregion

            #region Constructors

            public ParentValue(object attachedParent, IBindingMemberInfo parentMember)
            {
                _attachedParent = attachedParent;
                _parentMember = parentMember;
            }

            #endregion

            #region Methods

            public ParentValue UpdateAttachedParent(object source, object attachedParent, object[] args)
            {
                if (_parentMember != null && _parentMember.CanWrite &&
                    (attachedParent == null || _parentMember.Type.IsInstanceOfType(attachedParent)))
                    _parentMember.SetValue(source, args);
                return new ParentValue(attachedParent, _parentMember);
            }

            public object GetParent(object source)
            {
                if (_parentMember == null)
                    return _attachedParent;
                return _parentMember.GetValue(source, Empty.Array<object>()) ?? _attachedParent;
            }

            #endregion
        }

        private sealed class ExplicitParentListener : IEventListener
        {
            #region Fields

            private static readonly ExplicitParentListener Instance;

            #endregion

            #region Constructors

            static ExplicitParentListener()
            {
                Instance = new ExplicitParentListener();
            }

            private ExplicitParentListener()
            {
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive => true;

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                AttachedParentMember.Raise(sender, message);
                return true;
            }

            #endregion

            #region Methods

            public static bool SetParentValue(object o)
            {
                var member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(o.GetType(), AttachedMemberConstants.ParentExplicit, false, false) ??
                             BindingServiceProvider
                                 .MemberProvider
                                 .GetBindingMember(o.GetType(), AttachedMemberConstants.Parent, true, false);
                if (member == null)
                    return false;
                AttachedParentMember.SetValue(o, new ParentValue(null, member));
                member.TryObserve(o, Instance);
                return true;
            }

            #endregion

        }

        #endregion

        #region Fields

        private static bool _isLoaded;
        private static readonly HashSet<Type> ExplicitParentTypes;
        private static readonly bool DebbugerAttached;
        private static readonly IAttachedBindingMemberInfo<object, object> CommandParameterInternal;
        private static readonly INotifiableAttachedBindingMemberInfo<object, ParentValue> AttachedParentMember;
        private const string ErrorsKey = "@!err";

        #endregion

        #region Constructors

        static DataBindingModule()
        {
            DebbugerAttached = Debugger.IsAttached;
            ExplicitParentTypes = new HashSet<Type>();
            CommandParameterInternal = AttachedBindingMember.CreateAutoProperty<object, object>("~#@cmdparam");
            AttachedParentMember = AttachedBindingMember.CreateAutoProperty<object, ParentValue>("#" + AttachedMemberConstants.Parent);
        }

        #endregion

        #region Implementation of IModule

        public virtual int Priority => ModuleBase.BindingModulePriority;

        public bool Load(IModuleContext context)
        {
            Should.NotBeNull(context, nameof(context));
            if (!CanLoad(context))
                return false;

            InitilaizeServices(context);
            var assemblies = context.Assemblies;
            for (int i = 0; i < assemblies.Count; i++)
            {
                var assembly = assemblies[i];
                if (CanRegisterTypes(assembly))
                {
                    var types = assembly.SafeGetTypes(!context.Mode.IsDesignMode());
                    for (int j = 0; j < types.Count; j++)
                        RegisterType(types[j]);
                }
            }
            if (!_isLoaded)
            {
                RegisterDefaultMembers();
                _isLoaded = true;
            }

            OnLoaded(context);
            return true;
        }

        public void Unload(IModuleContext context)
        {
            OnUnloaded(context);
        }

        #endregion

        #region Methods

        protected virtual bool CanLoad(IModuleContext context)
        {
            return true;
        }

        protected virtual void OnLoaded(IModuleContext context)
        {
        }

        protected virtual void OnUnloaded(IModuleContext context)
        {
        }

        protected virtual void RegisterType(Type type)
        {
            if (_isLoaded)
                return;
            var isConverter = typeof(IBindingValueConverter).IsAssignableFrom(type);
            var isTemplate = typeof(IDataTemplateSelector).IsAssignableFrom(type);

            if ((!isConverter && !isTemplate) || !type.IsPublicNonAbstractClass())
                return;

            if (BindingServiceProvider.DisableConverterAutoRegistration && isConverter)
                return;
            if (BindingServiceProvider.DisableDataTemplateSelectorAutoRegistration && isTemplate)
                return;

            var constructor = type.GetConstructor(Empty.Array<Type>());
            if (constructor == null || !constructor.IsPublic)
                return;

            var value = constructor.InvokeEx();
            if (isTemplate)
                BindingServiceProvider.ResourceResolver.AddObject(type.Name, value, true);
            else
                BindingServiceProvider.ResourceResolver.AddConverter((IBindingValueConverter)value, type, true);

            if (Tracer.TraceInformation)
                Tracer.Info("The {0} is registered.", type);
        }

        protected virtual bool CanRegisterTypes(Assembly assembly)
        {
            return assembly.IsToolkitAssembly();
        }

        [CanBeNull]
        protected virtual IBindingErrorProvider GetBindingErrorProvider(IModuleContext context)
        {
            return new BindingErrorProviderBase();
        }

        [CanBeNull]
        protected virtual IBindingProvider GetBindingProvider(IModuleContext context)
        {
            return null;
        }

        [CanBeNull]
        protected virtual IBindingManager GetBindingManager(IModuleContext context)
        {
            return null;
        }

        [CanBeNull]
        protected virtual IBindingMemberProvider GetBindingMemberProvider(IModuleContext context)
        {
            return null;
        }

        [CanBeNull]
        protected virtual IObserverProvider GetObserverProvider(IModuleContext context)
        {
            return null;
        }

        [CanBeNull]
        protected virtual IBindingContextManager GetBindingContextManager(IModuleContext context)
        {
            return null;
        }

        [CanBeNull]
        protected virtual IBindingResourceResolver GetBindingResourceResolver(IModuleContext context)
        {
            return null;
        }

        [CanBeNull]
        protected virtual IVisualTreeManager GetVisualTreeManager(IModuleContext context)
        {
            return null;
        }

        [CanBeNull]
        protected virtual IWeakEventManager GetWeakEventManager(IModuleContext context)
        {
            return null;
        }

        private void InitilaizeServices(IModuleContext context)
        {
            var bindingProvider = GetBindingProvider(context);
            if (bindingProvider != null)
                BindingServiceProvider.BindingProvider = bindingProvider;

            var bindingManager = GetBindingManager(context);
            if (bindingManager != null)
                BindingServiceProvider.BindingManager = bindingManager;

            var memberProvider = GetBindingMemberProvider(context);
            if (memberProvider != null)
                BindingServiceProvider.MemberProvider = memberProvider;

            var observerProvider = GetObserverProvider(context);
            if (observerProvider != null)
                BindingServiceProvider.ObserverProvider = observerProvider;

            var contextManager = GetBindingContextManager(context);
            if (contextManager != null)
                BindingServiceProvider.ContextManager = contextManager;

            var resourceResolver = GetBindingResourceResolver(context);
            if (resourceResolver != null)
                BindingServiceProvider.ResourceResolver = resourceResolver;

            var visualTreeManager = GetVisualTreeManager(context);
            if (visualTreeManager != null)
                BindingServiceProvider.VisualTreeManager = visualTreeManager;

            var weakEventManager = GetWeakEventManager(context);
            if (weakEventManager != null)
                BindingServiceProvider.WeakEventManager = weakEventManager;

            var errorProvider = GetBindingErrorProvider(context);
            if (errorProvider != null)
                BindingServiceProvider.ErrorProvider = errorProvider;
        }

        private static IDisposable ObserveParent(IBindingMemberInfo bindingMemberInfo, object o, IEventListener arg3)
        {
            return AttachedParentMember.TryObserve(o, arg3);
        }

        private static object SetParent(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            var value = AttachedParentMember.GetValue(o, null);
            return AttachedParentMember.SetValue(o, value.UpdateAttachedParent(o, arg3[0], arg3));
        }

        private static object GetParent(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            return AttachedParentMember.GetValue(o, arg3).GetParent(o);
        }

        private static void ParentAttached(object o, MemberAttachedEventArgs args)
        {
            if (!ExplicitParentListener.SetParentValue(o) && DebbugerAttached)
            {
                lock (ExplicitParentTypes)
                {
                    var type = o.GetType();
                    if (!ExplicitParentTypes.Contains(type))
                    {
                        Tracer.Warn(@"Could not find a 'Parent' property on type '{0}', you should register it, without this the data bindings may not work properly. You can ignore this message if you are using the attached parent member.", type);
                        ExplicitParentTypes.Add(type);
                    }
                }
            }
        }

        private static IDisposable ObserveRootMember(IBindingMemberInfo member, object o, IEventListener arg3)
        {
            var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(o.GetType());
            if (rootMember == null)
                return null;
            return rootMember.TryObserve(o, arg3);
        }

        private static object GetRootMember(IBindingMemberInfo member, object o, object[] arg3)
        {
            var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(o.GetType());
            if (rootMember == null)
                return null;
            return rootMember.GetValue(o, arg3);
        }

        private static void RegisterDefaultMembers()
        {
            ViewManager.GetDataContext = BindingExtensions.DataContext;
            ViewManager.SetDataContext = BindingExtensions.SetDataContext;

            var memberProvider = BindingServiceProvider.MemberProvider;

            var registration = new DefaultAttachedMemberRegistration<object>(CommandParameterInternal, AttachedMemberConstants.CommandParameter);
            memberProvider.Register(registration.ToAttachedBindingMember<object>());
            var parentMember = AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.Parent, GetParent, SetParent, ObserveParent, ParentAttached);
            AttachedBindingMember.TrySetRaiseAction(parentMember, (info, o, arg3) => AttachedParentMember.Raise(o, arg3));
            memberProvider.Register(parentMember);
            memberProvider.Register(AttachedBindingMember.CreateMember<object, object>("Root", GetRootMember, null, ObserveRootMember));

            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember<object, IEnumerable<object>>(AttachedMemberConstants.ErrorsPropertyMember, GetErrors, SetErrors));
            memberProvider.Register(AttachedBindingMember.CreateMember<object, bool>("HasErrors", GetHasErrors, null, ObserveHasErrors));
        }

        private static bool SetErrors(IBindingMemberInfo bindingMemberInfo, object o, IEnumerable<object> errors)
        {
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return false;
            var errorsList = errors as IList<object>;
            if (errorsList == null)
                errorsList = errors == null ? Empty.Array<object>() : errors.ToArray();
            errorProvider.SetErrors(o, ErrorsKey, errorsList, DataContext.Empty);
            return true;
        }

        private static IEnumerable<object> GetErrors(IBindingMemberInfo bindingMemberInfo, object o)
        {
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return Empty.Array<object>();
            return errorProvider.GetErrors(o, string.Empty, DataContext.Empty);
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

        #endregion
    }
}
