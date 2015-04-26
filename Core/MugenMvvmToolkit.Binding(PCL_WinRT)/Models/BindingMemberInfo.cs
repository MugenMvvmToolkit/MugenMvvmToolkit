#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberInfo.cs">
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
using System.Dynamic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Accessors;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    internal sealed class BindingMemberInfo : IBindingMemberInfo
    {
        #region Nested types

        private sealed class ArrayAccessor
        {
            #region Fields

            private readonly int[] _indexes;

            #endregion

            #region Constructors

            public ArrayAccessor(int[] indexes)
            {
                _indexes = indexes;
            }

            #endregion

            #region Methods

            public object GetValue(object arg)
            {
                return ((Array)arg).GetValue(_indexes);
            }

            public void SetValue(object arg1, object arg2)
            {
                ((Array)arg1).SetValue(arg2, _indexes);
            }

            #endregion
        }

        private sealed class DynamicObjectAccessor
        {
            #region Fields

            private readonly string _path;
            private readonly IList<object> _indexerValues;

            #endregion

            #region Constructors

            public DynamicObjectAccessor(string path, IList<object> indexerValues)
            {
                _path = path;
                _indexerValues = indexerValues;
            }

            #endregion

            #region Methods

            public object GetValue(object arg, object[] args)
            {
                return ((IDynamicObject)arg).GetMember(_path, args);
            }

            public object SetValue(object arg1, object[] args)
            {
                ((IDynamicObject)arg1).SetMember(_path, args);
                return null;
            }

            public object GetValueIndex(object arg, object[] args)
            {
                return ((IDynamicObject)arg).GetIndex(_indexerValues, DataContext.Empty);
            }

            public object SetValueIndex(object arg1, object[] args)
            {
                ((IDynamicObject)arg1).SetIndex(_indexerValues, DataContext.Empty);
                return null;
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Gets the binding context member.
        /// </summary>
        public static readonly BindingMemberInfo BindingContextMember;

        /// <summary>
        ///     Gets the empty member.
        /// </summary>
        public static readonly IBindingMemberInfo Empty;

        /// <summary>
        ///     Gets the empty member.
        /// </summary>
        public static readonly IBindingMemberInfo EmptyHasSetter;

        /// <summary>
        ///     Gets the unset member.
        /// </summary>
        public static readonly IBindingMemberInfo Unset;

        internal static readonly IBindingMemberInfo MultiBindingSourceAccessorMember;

        private readonly bool _canRead;
        private readonly bool _canWrite;
        private readonly bool _isDataContext;
        private readonly bool _isDynamic;
        private readonly bool _isSingleParameter;
        private readonly Func<object, object[], object> _getValueAccessor;
        private readonly Func<object, object> _getValueAccessorSingle;
        private readonly Func<object, object, object> _setValueAccessorSingle;
        private readonly Action<object, object> _setValueAccessorSingleAction;
        private readonly Func<object, object[], object> _setValueAccessor;
        private readonly MemberInfo _member;
        private readonly BindingMemberType _memberType;
        private readonly string _path;
        private readonly Type _type;

        private readonly IBindingMemberInfo _memberEvent;

        #endregion

        #region Constructors

        static BindingMemberInfo()
        {
            BindingContextMember = new BindingMemberInfo(AttachedMemberConstants.DataContext, BindingMemberType.BindingContext);
            Empty = new BindingMemberInfo("Empty", BindingMemberType.Empty);
            EmptyHasSetter = new BindingMemberInfo("Empty", BindingMemberType.Empty, true);
            Unset = new BindingMemberInfo("Unset", BindingMemberType.Unset);
            MultiBindingSourceAccessorMember = new BindingMemberInfo();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        private BindingMemberInfo(string path, BindingMemberType memberType, Type type)
        {
            Should.NotBeNull(path, "path");
            Should.NotBeNull(type, "type");
            _type = type;
            _memberType = memberType;
            _path = path;
        }

        private BindingMemberInfo()
            : this("$multiValue", BindingMemberType.Attached, typeof(object))
        {
            _getValueAccessor = (o, objects) =>
            {
                IDataContext context = null;
                if (objects.Length == 3)
                    context = objects[2] as IDataContext;
                if (context == null)
                    context = objects.OfType<IDataContext>().FirstOrDefault() ?? DataContext.Empty;
                return ((MultiBindingSourceAccessor)o).GetRawValueInternal(context);
            };
            _setValueAccessor = _getValueAccessor;
            _canRead = true;
            _canWrite = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        private BindingMemberInfo(string path, BindingMemberType memberType, bool hasSetter = false)
            : this(path, memberType, typeof(object))
        {
            if (memberType == BindingMemberType.BindingContext)
            {
                _isDataContext = true;
                _getValueAccessorSingle = o => BindingServiceProvider.ContextManager.GetBindingContext(o).Value;
                _setValueAccessorSingle = (o, arg) => BindingServiceProvider.ContextManager.GetBindingContext(o).Value = arg;
                _canRead = true;
                _canWrite = true;
                _isSingleParameter = true;
            }
            else if (memberType == BindingMemberType.Empty)
            {
                if (hasSetter)
                {
                    _getValueAccessor = (o, objects) => null;
                    _setValueAccessor = (o, objects) => null;
                }
                else
                {
                    _getValueAccessorSingle = o => o;
                    _setValueAccessorSingle = NotSupportedSetter;
                    _isSingleParameter = true;
                }
                _canRead = true;
                _canWrite = _setValueAccessorSingle != null;
            }
            else if (memberType == BindingMemberType.Unset)
            {
                _getValueAccessorSingle = o => BindingConstants.UnsetValue;
                _setValueAccessorSingle = NotSupportedSetter;
                _canRead = true;
                _canWrite = false;
                _isSingleParameter = true;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        public BindingMemberInfo([NotNull] string path, [NotNull] FieldInfo field, Type sourceType)
            : this(path, BindingMemberType.Field, field.FieldType)
        {
            _member = field;
            _getValueAccessorSingle = ServiceProvider.ReflectionManager.GetMemberGetter<object>(field);
            _setValueAccessorSingleAction = ServiceProvider.ReflectionManager.GetMemberSetter<object>(field);
            _canRead = true;
            _canWrite = true;
            _isSingleParameter = true;
            _memberEvent = BindingExtensions.TryFindMemberChangeEvent(BindingServiceProvider.MemberProvider, sourceType, field.Name);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        public BindingMemberInfo([NotNull] string path, [NotNull] PropertyInfo property, Type sourceType)
            : this(path, BindingMemberType.Property, property.PropertyType)
        {
            _member = property;
            var method = property.GetGetMethod(true);
            if (method == null)
            {
                _getValueAccessorSingle = NotSupportedGetter;
                _canRead = false;
            }
            else
            {
                _getValueAccessorSingle = property.GetGetPropertyAccessor(method, path);
                _canRead = true;
            }
            method = property.GetSetMethod(true);
            if (method == null)
            {
                _setValueAccessorSingle = NotSupportedSetter;
                _canWrite = false;
            }
            else
            {
                _setValueAccessorSingleAction = property.GetSetPropertyAccessor(method, path);
                _canWrite = true;
            }
            _isSingleParameter = true;

            _memberEvent = BindingExtensions.TryFindMemberChangeEvent(BindingServiceProvider.MemberProvider, sourceType, property.Name);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        public BindingMemberInfo(string path, EventInfo @event)
            : this(path, BindingMemberType.Event, @event.EventHandlerType)
        {
            _member = @event;
            _getValueAccessorSingle = GetBindingMemberValue;
            _setValueAccessorSingle = SetEventValue;
            _canRead = true;
            _canWrite = true;
            _isSingleParameter = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        public BindingMemberInfo(string path, Type type)
            : this(path, BindingMemberType.Array, type.GetElementType())
        {
            var indexes = BindingReflectionExtensions
                .GetIndexerValues(path, null, typeof(int))
                .ToArrayEx(o => (int)o);
            var arrayAccessor = new ArrayAccessor(indexes);
            _getValueAccessorSingle = arrayAccessor.GetValue;
            _setValueAccessorSingleAction = arrayAccessor.SetValue;

            _canRead = true;
            _canWrite = true;
            _isSingleParameter = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        public BindingMemberInfo(string path, bool expandoObject)
            : this(path, BindingMemberType.Dynamic, typeof(object))
        {
            object[] indexerValues = null;
            if (expandoObject)
            {
                _isSingleParameter = true;
                _getValueAccessorSingle = o =>
                {
                    object value;
                    ((IDictionary<string, object>)o).TryGetValue(path, out value);
                    return value;
                };
                _setValueAccessorSingleAction = (o, v) => ((IDictionary<string, object>)o)[path] = v;
            }
            else
            {
                if (path.StartsWith("Item[", StringComparison.Ordinal) || path.StartsWith("[", StringComparison.Ordinal))
                    indexerValues = BindingReflectionExtensions.GetIndexerValues(path, castType: typeof(string));
                var accessor = new DynamicObjectAccessor(path, indexerValues);
                if (indexerValues == null)
                {
                    _getValueAccessor = accessor.GetValue;
                    _setValueAccessor = accessor.SetValue;
                }
                else
                {
                    _getValueAccessor = accessor.GetValueIndex;
                    _setValueAccessor = accessor.SetValueIndex;
                }
                _isDynamic = true;
            }
            _canRead = true;
            _canWrite = true;
        }

        #endregion

        #region Implementation of IBindingMemberInfo

        /// <summary>
        ///     Gets the path of member.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        ///     Gets the type of member.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>
        ///     Gets the underlying member.
        /// </summary>
        public MemberInfo Member
        {
            get { return _member; }
        }

        /// <summary>
        ///     Gets the member type.
        /// </summary>
        public BindingMemberType MemberType
        {
            get { return _memberType; }
        }

        /// <summary>
        ///     Gets a value indicating whether the member can be read.
        /// </summary>
        public bool CanRead
        {
            get { return _canRead; }
        }

        /// <summary>
        ///     Gets a value indicating whether the property can be written to.
        /// </summary>
        public bool CanWrite
        {
            get { return _canWrite; }
        }

        /// <summary>
        ///     Returns the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be returned.</param>
        /// <param name="args">Optional values for members.</param>
        /// <returns>The member value of the specified object.</returns>
        public object GetValue(object source, object[] args)
        {
            if (_isSingleParameter)
                return _getValueAccessorSingle(source);
            return _getValueAccessor(source, args);
        }

        /// <summary>
        ///     Sets the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be set.</param>
        /// <param name="args">Optional values for members..</param>
        public object SetValue(object source, object[] args)
        {
            if (_isSingleParameter)
            {
                if (_setValueAccessorSingleAction == null)
                    return _setValueAccessorSingle(source, args[0]);
                _setValueAccessorSingleAction(source, args[0]);
                return null;
            }
            return _setValueAccessor(source, args);
        }

        /// <summary>
        ///     Attempts to track the value change.
        /// </summary>
        public IDisposable TryObserve(object source, IEventListener listener)
        {
            if (_isDataContext)
                return WeakEventManager.GetBindingContextListener(source).AddWithUnsubscriber(listener);
            if (_memberEvent == null)
            {
                if (_isDynamic)
                    return ((IDynamicObject)source).TryObserve(_path, listener);
                return null;
            }
            return _memberEvent.SetValue(source, new object[] { listener }) as IDisposable;
        }

        #endregion

        #region Methods

        private object NotSupportedGetter(object o)
        {
            throw BindingExceptionManager.BindingMemberMustBeReadable(this);
        }

        private object NotSupportedSetter(object o, object arg)
        {
            throw BindingExceptionManager.BindingMemberMustBeWriteable(this);
        }

        private object GetBindingMemberValue(object o)
        {
            return new BindingMemberValue(o, this);
        }

        private object SetEventValue(object o, object arg)
        {
            var listener = arg as IEventListener;
            if (listener == null)
                throw BindingExceptionManager.BindingMemberMustBeWriteable(this);
            return BindingServiceProvider.WeakEventManager.TrySubscribe(o, (EventInfo)_member, listener);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}", MemberType, Member);
        }

        #endregion
    }
}