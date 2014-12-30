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
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.DataConstants;
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

        private sealed class RootResourceObject : IBindingResourceObject, IEventListener
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

            #region Implementation of IBindingResourceObject

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

            public Type Type
            {
                get
                {
                    var value = Value;
                    return value == null ? typeof(object) : value.GetType();
                }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion
        }

        #endregion

        #region Fields

        private readonly Dictionary<string, IBindingValueConverter> _converters;
        private readonly Dictionary<string, IBindingResourceMethod> _dynamicMethods;
        private readonly Dictionary<string, IBindingResourceObject> _objects;
        private readonly Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>> _behaviors;
        private readonly Dictionary<string, Type> _types;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceResolver" /> class.
        /// </summary>
        public BindingResourceResolver()
        {
            _behaviors = new Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>>();
            _converters = new Dictionary<string, IBindingValueConverter>();
            _dynamicMethods = new Dictionary<string, IBindingResourceMethod>
            {
                {"Format", new BindingResourceMethod(FormatImpl, typeof (string))},
                {DefaultBindingParserHandler.GetEventArgsMethod, new BindingResourceMethod(GetEventArgs, GetEventArgsReturnType)},
                {DefaultBindingParserHandler.GetErrorsMethod, new BindingResourceMethod(GetErrorsMethod, typeof(IList<object>))}
            };
            _objects = new Dictionary<string, IBindingResourceObject>();
            _types = new Dictionary<string, Type>
            {
                {"Object", typeof (Object)},
                {"object", typeof (Object)},
                {"Boolean", typeof (Boolean)},
                {"bool", typeof (Boolean)},
                {"Char", typeof (Char)},
                {"char", typeof (Char)},
                {"String", typeof (String)},
                {"string", typeof (String)},
                {"SByte", typeof (SByte)},
                {"sbyte", typeof (SByte)},
                {"Byte", typeof (Byte)},
                {"byte", typeof (Byte)},
                {"Int16", typeof (Int16)},
                {"short", typeof (Int16)},
                {"UInt16", typeof (UInt16)},
                {"ushort", typeof (UInt16)},
                {"Int32", typeof (Int32)},
                {"int", typeof (Int32)},
                {"UInt32", typeof (UInt32)},
                {"uint", typeof (UInt32)},
                {"Int64", typeof (Int64)},
                {"long", typeof (Int64)},
                {"UInt64", typeof (UInt64)},
                {"ulong", typeof (UInt64)},
                {"Single", typeof (Single)},
                {"float", typeof (Single)},
                {"Double", typeof (Double)},
                {"double", typeof (Double)},
                {"Decimal", typeof (Decimal)},
                {"decimal", typeof (Decimal)},
                {"DateTime", typeof (DateTime)},
                {"TimeSpan", typeof (TimeSpan)},
                {"Guid", typeof (Guid)},
                {"Math", typeof (Math)},
                {"Convert", typeof (Convert)},
                {"Enumerable", typeof (Enumerable)},
                {"Environment", typeof(Environment)}
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingResourceResolver"/> class.
        /// </summary>
        public BindingResourceResolver([NotNull] BindingResourceResolver resolver)
        {
            Should.NotBeNull(resolver, "resolver");
            _behaviors = new Dictionary<string, Func<IDataContext, IList<object>, IBindingBehavior>>(resolver._behaviors);
            _converters = new Dictionary<string, IBindingValueConverter>(resolver._converters);
            _dynamicMethods = new Dictionary<string, IBindingResourceMethod>(resolver._dynamicMethods);
            _objects = new Dictionary<string, IBindingResourceObject>(resolver._objects);
            _types = new Dictionary<string, Type>(resolver._types);
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
                return data.TargetAccessor.Source.GetSource(false);
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
            var binding = arg3.GetData(BindingConstants.Binding);
            if (binding == null)
                return Empty.Array<object>();
            var behavior = binding.Behaviors.OfType<NotifyDataErrorsAggregatorBehavior>().FirstOrDefault();
            if (behavior == null)
                return Empty.Array<object>();
            return behavior.Errors;
        }

        #endregion

        #region Implementation of IExpressionMemberResolver

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
        ///     Gets an instance of <see cref="IBindingResourceObject" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingResourceMethod" />.</returns>
        public virtual IBindingResourceObject ResolveObject(string name, IDataContext context, bool throwOnError)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            lock (_objects)
            {
                IBindingResourceObject value;
                if (!_objects.TryGetValue(name, out value))
                {
                    if ("root".Equals(name, StringComparison.OrdinalIgnoreCase) ||
                        "rootElement".Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        var target = TryGetTarget(context);
                        if (target != null)
                        {
                            var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(target.GetType());
                            if (rootMember != null)
                                return new RootResourceObject(target, rootMember);
                        }
                    }
                    if (throwOnError)
                        throw BindingExceptionManager.CannotResolveInstanceByName(this, "resource object", name);
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
        public virtual void AddObject(string name, IBindingResourceObject obj, bool rewrite)
        {
            Should.NotBeNullOrWhitespace(name, "name");
            Should.NotBeNull(obj, "obj");
            lock (_objects)
            {
                if (rewrite)
                    _objects[name] = obj;
                else
                    _objects.Add(name, obj);
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