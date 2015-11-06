#region Copyright

// ****************************************************************************
// <copyright file="BindingResourceResolver.cs">
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
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Extensions.Syntax;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindingResourceResolver : IBindingResourceResolver
    {
        #region Nested types

        private sealed class RootResourceObject : ISourceValue, IEventListener
        {
            #region Fields

            private readonly WeakReference _target;
            private readonly IBindingMemberInfo _rootMemberInfo;

            #endregion

            #region Constructors

            public RootResourceObject([NotNull] object target, [NotNull] IBindingMemberInfo rootMemberInfo)
            {
                _target = ToolkitExtensions.GetWeakReference(target);
                _rootMemberInfo = rootMemberInfo;
                rootMemberInfo.TryObserve(target, this);
            }

            #endregion

            #region Implementation of ISourceValue

            public bool IsAlive
            {
                get { return _target.Target != null; }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                var changed = ValueChanged;
                if (changed != null)
                    changed(this, EventArgs.Empty);
                return IsAlive;
            }

            public object Value
            {
                get
                {
                    var target = _target.Target;
                    if (target == null)
                        return null;
                    return _rootMemberInfo.GetValue(target, null);
                }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion
        }

        private sealed class DynamicResourceObject : ISourceValue
        {
            #region Fields

            private ISourceValue _value;

            #endregion

            #region Implementation of ISourceValue

            public bool IsAlive
            {
                get { return true; }
            }

            public object Value
            {
                get
                {
                    var value = _value;
                    if (value == null)
                        return null;
                    return value.Value;
                }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            public void SetValue(ISourceValue value, string name, bool rewrite)
            {
                var oldValue = _value;
                if (oldValue != null)
                {
                    if (!rewrite)
                        throw ExceptionManager.ObjectInitialized("resource", oldValue.Value, "Name - " + name);
                    oldValue.ValueChanged -= OnValueChanged;
                }
                _value = value;
                if (value != null)
                    value.ValueChanged += OnValueChanged;
                OnValueChanged(null, EventArgs.Empty);
            }

            private void OnValueChanged(ISourceValue sender, EventArgs args)
            {
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, args);
            }

            #endregion

        }

        private sealed class ConstResourceObject : ISourceValue
        {
            #region Fields

            private readonly WeakReference _reference;

            #endregion

            #region Constructors

            public ConstResourceObject(object value)
            {
                _reference = value as WeakReference ?? ToolkitExtensions.GetWeakReference(value);
            }

            #endregion

            #region Implementation of ISourceValue

            public bool IsAlive
            {
                get { return _reference.Target != null; }
            }

            public object Value
            {
                get { return _reference.Target; }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged
            {
                add { }
                remove { }
            }

            #endregion

        }

        #endregion

        #region Fields

        private const string ResourcePrefix = "@$res.";
        private readonly Dictionary<string, IBindingValueConverter> _converters;
        private readonly Dictionary<string, IBindingResourceMethod> _dynamicMethods;
        private readonly Dictionary<string, DynamicResourceObject> _objects;
        private readonly Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>> _behaviors;
        private readonly Dictionary<string, Type> _types;
        private readonly Dictionary<string, KeyValuePair<Type, string>> _aliasToMethod;
        private string _bindingSourceResourceName;
        private string _rootElementResourceName;
        private string _selfResourceName;
        private string _dataContextResourceName;

        #endregion

        #region Constructors

        public BindingResourceResolver()
        {
            BindingSourceResourceName = "src";
            RootElementResourceName = "root";
            SelfResourceName = "self";
            DataContextResourceName = "context";
            _behaviors = new Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>>();
            _converters = new Dictionary<string, IBindingValueConverter>();
            _dynamicMethods = new Dictionary<string, IBindingResourceMethod>
            {
                {DefaultBindingParserHandler.GetEventArgsMethod, new BindingResourceMethod(GetEventArgs, GetEventArgsReturnType)},
                {DefaultBindingParserHandler.GetErrorsMethod, new BindingResourceMethod(GetErrorsMethod, typeof (IList<object>))}
            };
            _objects = new Dictionary<string, DynamicResourceObject>();
            _aliasToMethod = new Dictionary<string, KeyValuePair<Type, string>>
            {
                {"Format", new KeyValuePair<Type, string>(typeof (string), "Format")},
                {"Equals", new KeyValuePair<Type, string>(typeof (object), "Equals")},
                {"ReferenceEquals", new KeyValuePair<Type, string>(typeof (object), "ReferenceEquals")}
            };
            _types = new Dictionary<string, Type>
            {
                {"object", typeof (Object)},
                {"bool", typeof (Boolean)},
                {"char", typeof (Char)},
                {"string", typeof (String)},
                {"sbyte", typeof (SByte)},
                {"byte", typeof (Byte)},
                {"short", typeof (Int16)},
                {"ushort", typeof (UInt16)},
                {"int", typeof (Int32)},
                {"uint", typeof (UInt32)},
                {"long", typeof (Int64)},
                {"ulong", typeof (UInt64)},
                {"float", typeof (Single)},
                {"double", typeof (Double)},
                {"decimal", typeof (Decimal)},
            };
            //to reduce constant string size.
            this.AddType(typeof(Object));
            this.AddType(typeof(Boolean));
            this.AddType(typeof(Char));
            this.AddType(typeof(String));
            this.AddType(typeof(SByte));
            this.AddType(typeof(Byte));
            this.AddType(typeof(Int16));
            this.AddType(typeof(UInt16));
            this.AddType(typeof(Int32));
            this.AddType(typeof(UInt32));
            this.AddType(typeof(Int64));
            this.AddType(typeof(UInt64));
            this.AddType(typeof(Single));
            this.AddType(typeof(Double));
            this.AddType(typeof(Decimal));
            this.AddType(typeof(DateTime));
            this.AddType(typeof(TimeSpan));
            this.AddType(typeof(Guid));
            this.AddType(typeof(Math));
            this.AddType(typeof(Convert));
            this.AddType(typeof(Enumerable));
            this.AddType(typeof(Environment));
        }

        public BindingResourceResolver([NotNull] BindingResourceResolver resolver)
        {
            Should.NotBeNull(resolver, "resolver");
            RootElementResourceName = resolver.RootElementResourceName;
            BindingSourceResourceName = resolver.BindingSourceResourceName;
            SelfResourceName = resolver.SelfResourceName;
            DataContextResourceName = resolver.DataContextResourceName;
            _behaviors = new Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>>(resolver._behaviors);
            _converters = new Dictionary<string, IBindingValueConverter>(resolver._converters);
            _dynamicMethods = new Dictionary<string, IBindingResourceMethod>(resolver._dynamicMethods);
            _objects = new Dictionary<string, DynamicResourceObject>(resolver._objects);
            _types = new Dictionary<string, Type>(resolver._types);
            _aliasToMethod = new Dictionary<string, KeyValuePair<Type, string>>(resolver._aliasToMethod);
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected static object TryGetTarget([CanBeNull] IDataContext context)
        {
            if (context == null)
                return null;
            object target;
            if (context.TryGetData(BindingBuilderConstants.Target, out target))
                return target;
            IDataBinding data;
            if (context.TryGetData(BindingConstants.Binding, out data))
                return data.TargetAccessor.Source.GetActualSource(false);
            return null;
        }

        [NotNull]
        protected ISourceValue GetOrAddDynamicResource([NotNull] string name, bool traceWarn)
        {
            lock (_objects)
            {
                DynamicResourceObject value;
                if (!_objects.TryGetValue(name, out value))
                {
                    if (traceWarn && Tracer.TraceWarning)
                        Tracer.Warn(BindingExceptionManager.CannotResolveInstanceFormat2, "resource", name, GetType().Name);
                    value = new DynamicResourceObject();
                    _objects[name] = value;
                }
                return value;
            }
        }

        private static Type GetEventArgsReturnType(IList<Type> types, IList<Type> list, IDataContext arg3)
        {
            if (arg3 == null)
                arg3 = DataContext.Empty;
            var dataBinding = arg3.GetData(BindingConstants.Binding);
            if (dataBinding == null)
                return typeof(object);
            var members = dataBinding.TargetAccessor.Source.GetPathMembers(true);
            var eventInfo = members.LastMember.Member as EventInfo;
            if (!members.AllMembersAvailable || eventInfo == null)
                return typeof(object);
            var invokeMethod = eventInfo.EventHandlerType.GetMethodEx("Invoke", MemberFlags.Instance | MemberFlags.Public);
            if (invokeMethod == null)
                return typeof(object);
            var parameters = invokeMethod.GetParameters();
            if (parameters.Length == 2)
                return parameters[1].ParameterType;
            return typeof(object);
        }

        private static object GetEventArgs(IList<Type> types, object[] items, IDataContext dataContext)
        {
            if (dataContext == null)
                dataContext = DataContext.Empty;
            return dataContext.GetData(BindingConstants.CurrentEventArgs);
        }

        private static object GetErrorsMethod(IList<Type> types, object[] objects, IDataContext arg3)
        {
            //The first argument must always be an identifier.
            return BindingSyntaxEx.GetErrorsImpl((Guid)objects[0], arg3, objects);
        }

        [CanBeNull]
        protected virtual ISourceValue ResolveObjectInternal([NotNull] object target, string name, IDataContext context, out bool keepValue)
        {
            keepValue = true;
            if (SelfResourceName.Equals(name, StringComparison.Ordinal))
                return new ConstResourceObject(target);
            if (DataContextResourceName.Equals(name, StringComparison.Ordinal))
            {
                keepValue = false;
                return BindingServiceProvider.ContextManager.GetBindingContext(target);
            }
            if (RootElementResourceName.Equals(name, StringComparison.Ordinal))
            {
                var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(target.GetType());
                if (rootMember != null)
                    return new RootResourceObject(target, rootMember);
            }
            return null;
        }

        private ISourceValue GetTargetResourceObject(string name, IDataContext context)
        {
            var target = TryGetTarget(context);
            if (target == null)
                return null;
            string key = ResourcePrefix + name;
            var value = ServiceProvider.AttachedValueProvider.GetValue<ISourceValue>(target, key, false);
            if (value != null)
                return value;
            bool keepValue;
            value = ResolveObjectInternal(target, name, context, out keepValue);
            if (keepValue)
                ServiceProvider.AttachedValueProvider.SetValue(target, key, value);
            return value;
        }

        #endregion

        #region Implementation of IBindingResourceResolver

        public string SelfResourceName
        {
            get { return _selfResourceName; }
            set
            {
                Should.PropertyNotBeNull(value);
                _selfResourceName = value;
            }
        }

        public string RootElementResourceName
        {
            get { return _rootElementResourceName; }
            set
            {
                Should.PropertyNotBeNull(value);
                _rootElementResourceName = value;
            }
        }

        public string BindingSourceResourceName
        {
            get { return _bindingSourceResourceName; }
            set
            {
                Should.PropertyNotBeNull(value);
                _bindingSourceResourceName = value;
            }
        }

        public string DataContextResourceName
        {
            get { return _dataContextResourceName; }
            set
            {
                Should.PropertyNotBeNull(value);
                _dataContextResourceName = value;
            }
        }

        public virtual IList<Type> GetKnownTypes()
        {
            lock (_types)
                return _types.Values.Distinct().ToList();
        }

        public virtual IBindingValueConverter ResolveConverter(string name, IDataContext context, bool throwOnError)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            lock (_converters)
            {
                IBindingValueConverter value;
                if (!_converters.TryGetValue(name, out value) && throwOnError)
                    throw BindingExceptionManager.CannotResolveInstanceByName(this, "converter", name);
                return value;
            }
        }

        public virtual Type ResolveType(string typeName, IDataContext context, bool throwOnError)
        {
            Type value = Type.GetType(typeName, false);
            if (value == null)
            {
                lock (_types)
                {
                    if (!_types.TryGetValue(typeName, out value) && throwOnError)
                        throw BindingExceptionManager.CannotResolveInstanceByName(this, "type", typeName);
                }
            }
            return value;
        }

        public virtual IBindingResourceMethod ResolveMethod(string name, IDataContext context, bool throwOnError)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            lock (_dynamicMethods)
            {
                IBindingResourceMethod value;
                if (!_dynamicMethods.TryGetValue(name, out value) && throwOnError)
                    throw BindingExceptionManager.CannotResolveInstanceByName(this, "dynamic method", name);
                return value;
            }
        }

        public virtual ISourceValue ResolveObject(string name, IDataContext context, bool throwOnError)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            if (context != null && BindingSourceResourceName.Equals(name, StringComparison.Ordinal))
            {
                object src;
                if (context.TryGetData(BindingBuilderConstants.Source, out src))
                    return src as ISourceValue ?? new ConstResourceObject(src);

                object target = null;
                IDataBinding binding;
                if (context.TryGetData(BindingConstants.Binding, out binding))
                {
                    WeakReference srcWeak;
                    if (binding.Context.TryGetData(BindingConstants.Source, out srcWeak))
                        return new ConstResourceObject(srcWeak);
                    target = binding.TargetAccessor.Source.GetActualSource(false);
                }
                if (target == null)
                    target = context.GetData(BindingBuilderConstants.Target);
                if (target != null)
                    return BindingServiceProvider.ContextManager.GetBindingContext(target);
            }
            var targetResourceObject = GetTargetResourceObject(name, context);
            if (targetResourceObject == null)
                return GetOrAddDynamicResource(name, true);
            return targetResourceObject;
        }

        public virtual IBindingBehavior ResolveBehavior(string name, IDataContext context, IList<object> args, bool throwOnError)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Func<IDataContext, IList<object>, IBindingBehavior> value;
            lock (_behaviors)
            {
                if (!_behaviors.TryGetValue(name, out value))
                {
                    if (throwOnError)
                        throw BindingExceptionManager.CannotResolveInstanceByName(this, "binding behavior", name);
                    return null;
                }
            }
            return value(context, args);
        }

        public virtual void AddBehavior(string name, Func<IDataContext, IList<object>, IBindingBehavior> getBehavior, bool rewrite)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Should.NotBeNull(getBehavior, "getBehavior");
            lock (_behaviors)
            {
                if (rewrite)
                    _behaviors[name] = getBehavior;
                else
                    _behaviors.Add(name, getBehavior);
            }
        }

        public virtual void AddConverter(string name, IBindingValueConverter converter, bool rewrite)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Should.NotBeNull(converter, "converter");
            lock (_converters)
            {
                if (rewrite)
                    _converters[name] = converter;
                else
                    _converters.Add(name, converter);
                if (name == "ViewModelToViewConverter")
                    _converters["GetView"] = converter;
            }
        }

        public virtual void AddType(string name, Type type, bool rewrite)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Should.NotBeNull(type, "type");
            lock (_types)
            {
                if (rewrite)
                    _types[name] = type;
                else
                    _types.Add(name, type);
            }
        }

        public virtual void AddMethod(string name, IBindingResourceMethod method, bool rewrite)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Should.NotBeNull(method, "method");
            lock (_dynamicMethods)
            {
                if (rewrite)
                    _dynamicMethods[name] = method;
                else
                    _dynamicMethods.Add(name, method);
            }
        }

        public virtual void AddObject(string name, ISourceValue obj, bool rewrite)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Should.NotBeNull(obj, "obj");
            DynamicResourceObject value;
            lock (_objects)
            {
                if (!_objects.TryGetValue(name, out value))
                {
                    value = new DynamicResourceObject();
                    _objects[name] = value;
                }
            }
            value.SetValue(obj, name, rewrite);
        }

        public virtual void AddMethodAlias(string bindingMethodName, Type type, string method, bool rewrite)
        {
            Should.NotBeNull(bindingMethodName, "bindingMethodName");
            Should.NotBeNull(type, "type");
            Should.NotBeNullOrEmpty(method, "method");
            var methods = type.GetMethodsEx(MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Static);
            if (methods.Count == 0)
                throw BindingExceptionManager.InvalidBindingMember(type, method);
            lock (_aliasToMethod)
            {
                var value = new KeyValuePair<Type, string>(type, method);
                if (rewrite)
                    _aliasToMethod[bindingMethodName] = value;
                else
                    _aliasToMethod.Add(bindingMethodName, value);
            }
        }

        public virtual bool TryGetMethodAlias(string bindingMethodName, out Type type, out string method)
        {
            Should.NotBeNull(bindingMethodName, "bindingMethodName");
            lock (_aliasToMethod)
            {
                KeyValuePair<Type, string> pair;
                if (_aliasToMethod.TryGetValue(bindingMethodName, out pair))
                {
                    type = pair.Key;
                    method = pair.Value;
                    return true;
                }
                type = null;
                method = null;
                return false;
            }
        }

        public virtual bool RemoveMethodAlias(string bindingMethodName)
        {
            lock (_aliasToMethod)
                return _aliasToMethod.Remove(bindingMethodName);
        }

        public virtual bool RemoveBehavior(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_behaviors)
                return _behaviors.Remove(name);
        }

        public virtual bool RemoveConverter(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_converters)
                return _converters.Remove(name);
        }

        public virtual bool RemoveType(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_types)
                return _types.Remove(name);
        }

        public virtual bool RemoveMethod(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_dynamicMethods)
                return _dynamicMethods.Remove(name);
        }

        public virtual bool RemoveObject(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_objects)
                return _objects.Remove(name);
        }

        #endregion
    }
}
