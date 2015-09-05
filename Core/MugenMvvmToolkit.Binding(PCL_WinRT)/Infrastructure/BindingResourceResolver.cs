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
    /// <summary>
    ///     Represents the dynamic resource resolver.
    /// </summary>
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
            private string _name;

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
                if (_value != null)
                {
                    if (!rewrite)
                        throw ExceptionManager.ObjectInitialized("resource", _value.Value, "Name - " + name);
                    _value.ValueChanged -= OnValueChanged;
                }
                _name = name;
                _value = value;
                if (value != null)
                    value.ValueChanged += OnValueChanged;
                OnValueChanged(null, EventArgs.Empty);
            }

            public void OnResourceAdded(ISourceValue value, string name)
            {
                if (_name == name)
                    SetValue(value, name, true);
            }

            private void OnValueChanged(ISourceValue sender, EventArgs args)
            {
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, args);
            }

            #endregion

        }

        #endregion

        #region Fields

        private const int MaxInstanceObject = 200;
        private const string ResourcePrefix = "@$res.";
        private readonly Dictionary<string, IBindingValueConverter> _converters;
        private readonly Dictionary<string, IBindingResourceMethod> _dynamicMethods;
        private readonly Dictionary<string, DynamicResourceObject> _objects;
        private readonly Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>> _behaviors;
        private readonly Dictionary<string, Type> _types;
        private readonly List<WeakReference> _instanceObjects;
        private string _bindingSourceResourceName;
        private string _rootElementResourceName;
        private string _selfResourceName;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceResolver" /> class.
        /// </summary>
        public BindingResourceResolver()
        {
            BindingSourceResourceName = "src";
            RootElementResourceName = "root";
            SelfResourceName = "self";
            _behaviors = new Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>>();
            _converters = new Dictionary<string, IBindingValueConverter>();
            _dynamicMethods = new Dictionary<string, IBindingResourceMethod>
            {
                {"Format", new BindingResourceMethod(FormatImpl, typeof (string))},
                {DefaultBindingParserHandler.GetEventArgsMethod, new BindingResourceMethod(GetEventArgs, GetEventArgsReturnType)},
                {DefaultBindingParserHandler.GetErrorsMethod, new BindingResourceMethod(GetErrorsMethod, typeof(IList<object>))}
            };
            _objects = new Dictionary<string, DynamicResourceObject>();
            _instanceObjects = new List<WeakReference>();
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceResolver" /> class.
        /// </summary>
        public BindingResourceResolver([NotNull] BindingResourceResolver resolver)
        {
            Should.NotBeNull(resolver, "resolver");
            RootElementResourceName = resolver.RootElementResourceName;
            BindingSourceResourceName = resolver.BindingSourceResourceName;
            SelfResourceName = resolver.SelfResourceName;
            _behaviors = new Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>>(resolver._behaviors);
            _converters = new Dictionary<string, IBindingValueConverter>(resolver._converters);
            _dynamicMethods = new Dictionary<string, IBindingResourceMethod>(resolver._dynamicMethods);
            _objects = new Dictionary<string, DynamicResourceObject>(resolver._objects);
            _types = new Dictionary<string, Type>(resolver._types);
            _instanceObjects = new List<WeakReference>(resolver._instanceObjects);
        }

        #endregion

        #region Methods

        protected static object TryGetTarget(IDataContext context)
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

        private static object FormatImpl(IList<Type> typeArgs, object[] args, IDataContext arg3)
        {
            if (args.Length == 0)
                return string.Empty;
            string format = (string)args[0] ?? string.Empty;
            if (args.Length == 1)
                return format;

            var formatItems = new object[args.Length - 1];
            Array.Copy(args, 1, formatItems, 0, formatItems.Length);
            return string.Format(format, formatItems);
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

        /// <summary>
        ///     Gets an instance of <see cref="ISourceValue" /> by the specified name.
        /// </summary>
        /// <param name="target">The binding target.</param>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        [CanBeNull]
        protected virtual ISourceValue ResolveObjectInternal([NotNull] object target, string name, IDataContext context)
        {
            if (SelfResourceName.Equals(name, StringComparison.Ordinal))
                return new BindingResourceObject(target, true);
            if (RootElementResourceName.Equals(name, StringComparison.Ordinal))
            {
                var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(target.GetType());
                if (rootMember != null)
                    return new RootResourceObject(target, rootMember);
            }
            return null;
        }

        private DynamicResourceObject GetTargetResourceObject(string name, IDataContext context)
        {
            var target = TryGetTarget(context);
            if (target == null)
                return null;

            string key = ResourcePrefix + name;
            var sourceValue = ServiceProvider.AttachedValueProvider.GetValue<DynamicResourceObject>(target, key, false);
            if (sourceValue == null)
            {
                var value = ResolveObjectInternal(target, name, context);
                if (value == null)
                    return null;
                sourceValue = new DynamicResourceObject();
                sourceValue.SetValue(value, name, true);
                ServiceProvider.AttachedValueProvider.SetValue(target, key, sourceValue);
                lock (_instanceObjects)
                {
                    if (_instanceObjects.Count > MaxInstanceObject)
                    {
                        for (int i = 0; i < _instanceObjects.Count; i++)
                        {
                            if (_instanceObjects[i].Target == null)
                            {
                                _instanceObjects.RemoveAt(i);
                                --i;
                            }
                        }
                    }
                    _instanceObjects.Add(ServiceProvider.WeakReferenceFactory(sourceValue));
                }
            }
            return sourceValue;
        }

        #endregion

        #region Implementation of IExpressionMemberResolver

        /// <summary>
        ///     Gets or sets the name of self element resource default is <c>self</c>.
        /// </summary>
        public string SelfResourceName
        {
            get { return _selfResourceName; }
            set
            {
                Should.PropertyNotBeNull(value);
                _selfResourceName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the name of root element resource default is <c>root</c>.
        /// </summary>
        public string RootElementResourceName
        {
            get { return _rootElementResourceName; }
            set
            {
                Should.PropertyNotBeNull(value);
                _rootElementResourceName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the name of binding source resource default is <c>src</c>.
        /// </summary>
        public string BindingSourceResourceName
        {
            get { return _bindingSourceResourceName; }
            set
            {
                Should.PropertyNotBeNull(value);
                _bindingSourceResourceName = value;
            }
        }

        /// <summary>
        ///     Gets a collection of known types.
        /// </summary>
        public virtual IList<Type> GetKnownTypes()
        {
            lock (_types)
                return _types.Values.Distinct().ToList();
        }

        /// <summary>
        ///     Gets an instance of <see cref="IBindingValueConverter" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingValueConverter" />.</returns>
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

        /// <summary>
        ///     Gets an instance of <see cref="Type" /> by the specified name.
        /// </summary>
        /// <param name="typeName">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="Type" />.</returns>
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

        /// <summary>
        ///     Gets an instance of <see cref="IBindingResourceMethod" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingResourceMethod" />.</returns>
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

        /// <summary>
        ///     Gets an instance of <see cref="ISourceValue" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="ISourceValue" />.</returns>
        public virtual ISourceValue ResolveObject(string name, IDataContext context, bool throwOnError)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            if (context != null && BindingSourceResourceName.Equals(name, StringComparison.Ordinal))
            {
                object src;
                if (context.TryGetData(BindingBuilderConstants.Source, out src))
                    return new BindingResourceObject(src, true);

                object target = null;
                IDataBinding binding;
                if (context.TryGetData(BindingConstants.Binding, out binding))
                {
                    WeakReference srcWeak;
                    if (binding.Context.TryGetData(BindingConstants.Source, out srcWeak))
                        return new BindingResourceObject(srcWeak);
                    target = binding.TargetAccessor.Source.GetActualSource(false);
                }
                if (target == null)
                    target = context.GetData(BindingBuilderConstants.Target);
                if (target != null)
                    return BindingServiceProvider.ContextManager.GetBindingContext(target);
            }
            lock (_objects)
            {
                DynamicResourceObject value;
                if (!_objects.TryGetValue(name, out value))
                {
                    var targetResourceObject = GetTargetResourceObject(name, context);
                    if (targetResourceObject != null)
                        return targetResourceObject;
                    if (Tracer.TraceWarning)
                        Tracer.Warn(BindingExceptionManager.CannotResolveInstanceFormat2, "resource", name, GetType().Name);
                    value = new DynamicResourceObject();
                    _objects[name] = value;
                }
                return value;
            }
        }

        /// <summary>
        ///     Gets an instance of <see cref="IBindingBehavior" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context.</param>
        /// <param name="args">The specified args to create behavior.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the object cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingBehavior" />.</returns>
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

        /// <summary>
        ///     Adds the specified behavior.
        /// </summary>
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

        /// <summary>
        ///     Adds the specified converter.
        /// </summary>
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
            }
        }

        /// <summary>
        ///     Adds the specified type.
        /// </summary>
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

        /// <summary>
        ///     Adds the specified method.
        /// </summary>
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

        /// <summary>
        ///     Adds the specified object.
        /// </summary>
        public virtual void AddObject(string name, ISourceValue obj, bool rewrite)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Should.NotBeNull(obj, "obj");
            lock (_objects)
            {
                DynamicResourceObject value;
                if (!_objects.TryGetValue(name, out value))
                {
                    value = new DynamicResourceObject();
                    _objects[name] = value;
                }
                value.SetValue(obj, name, rewrite);
            }
            lock (_instanceObjects)
            {
                for (int i = 0; i < _instanceObjects.Count; i++)
                {
                    var resourceObject = (DynamicResourceObject)_instanceObjects[i].Target;
                    if (resourceObject == null)
                    {
                        _instanceObjects.RemoveAt(i);
                        --i;
                    }
                    else
                        resourceObject.OnResourceAdded(obj, name);
                }
            }
        }

        /// <summary>
        ///     Removes the specified behavior using name.
        /// </summary>
        public virtual bool RemoveBehavior(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_behaviors)
                return _behaviors.Remove(name);
        }

        /// <summary>
        ///     Removes the specified converter using name.
        /// </summary>
        public bool RemoveConverter(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_converters)
                return _converters.Remove(name);
        }

        /// <summary>
        ///     Removes the specified type using name.
        /// </summary>
        public bool RemoveType(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_types)
                return _types.Remove(name);
        }

        /// <summary>
        ///     Removes the specified method using name.
        /// </summary>
        public bool RemoveMethod(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_dynamicMethods)
                return _dynamicMethods.Remove(name);
        }

        /// <summary>
        ///     Removes the specified object using name.
        /// </summary>
        public bool RemoveObject(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_objects)
                return _objects.Remove(name);
        }

        #endregion
    }
}