#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberInfo.cs">
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
using System.Collections.Generic;
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

        private sealed class AttachedIndexerAccessor
        {
            #region Fields

            private readonly IBindingMemberInfo _member;
            private readonly object[] _indexes;

            #endregion

            #region Constructors

            public AttachedIndexerAccessor(IBindingMemberInfo member, object[] indexes)
            {
                _member = member;
                _indexes = indexes;
            }

            #endregion

            #region Methods

            public object GetValue(object src, object[] args)
            {
                return _member.GetValue(src, BindingReflectionExtensions.InsertFirstArg(args ?? MugenMvvmToolkit.Empty.Array<object>(), _indexes));
            }

            public object SetValue(object src, object[] args)
            {
                return _member.SetValue(src, BindingReflectionExtensions.InsertFirstArg(args ?? MugenMvvmToolkit.Empty.Array<object>(), _indexes));
            }

            #endregion
        }

        #endregion

        #region Fields

        public static readonly BindingMemberInfo BindingContextMember;

        public static readonly IBindingMemberInfo Empty;

        public static readonly IBindingMemberInfo EmptyHasSetter;

        public static readonly IBindingMemberInfo Unset;

        internal static readonly IBindingMemberInfo MultiBindingSourceAccessorMember;

        private readonly bool _canRead;
        private readonly bool _canWrite;
        private readonly bool _canObserve;
        private readonly bool _isDataContext;
        private readonly bool _isDynamic;
        private readonly bool _isSingleParameter;
        private readonly Func<object, object[], object> _getValueAccessor;
        private Func<object, object> _getValueAccessorSingle;
        private readonly Func<object, object, object> _setValueAccessorSingle;
        private Action<object, object> _setValueAccessorSingleAction;
        private readonly Func<object, object[], object> _setValueAccessor;
        private readonly MemberInfo _member;
        private readonly BindingMemberType _memberType;
        private readonly string _path;
        private readonly Type _type;
        private readonly IBindingMemberInfo _observableMember;
        private readonly IBindingMemberInfo _memberEvent;
        private readonly IBindingMemberInfo _indexerAttachedBindingMember;

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

        private BindingMemberInfo(string path, BindingMemberType memberType, Type type)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(type, nameof(type));
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

        private BindingMemberInfo(string path, BindingMemberType memberType, bool hasSetter = false)
            : this(path, memberType, typeof(object))
        {
            if (memberType == BindingMemberType.BindingContext)
            {
                _canObserve = true;
                _isDataContext = true;
                _getValueAccessorSingle = o =>
                {
                    if (o == null)
                        return null;
                    return BindingServiceProvider.ContextManager.GetBindingContext(o).Value;
                };
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

        public BindingMemberInfo([NotNull] string path, [NotNull] FieldInfo field, Type sourceType)
            : this(path, BindingMemberType.Field, field.FieldType)
        {
            _member = field;
            _getValueAccessorSingle = InitiliazeFieldGetter;
            _setValueAccessorSingleAction = InitializeFieldSetter;
            _canRead = true;
            _canWrite = true;
            _isSingleParameter = true;
            _memberEvent = BindingServiceProvider.UpdateEventFinder(sourceType, field.Name);
            _canObserve = _memberEvent != null;
        }

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
                _getValueAccessorSingle = InitiliazePropertyGetter;
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
                _setValueAccessorSingleAction = InitiliazePropertySetter;
                _canWrite = true;
            }
            _isSingleParameter = true;
            _memberEvent = BindingServiceProvider.UpdateEventFinder(sourceType, property.Name);
            _canObserve = _memberEvent != null;
        }

        public BindingMemberInfo(string path, EventInfo @event, IBindingMemberInfo observableMember)
            : this(path, BindingMemberType.Event, @event == null ? typeof(Delegate) : @event.EventHandlerType)
        {
            _member = @event;
            _observableMember = observableMember;
            _getValueAccessorSingle = GetBindingMemberValue;
            if (@event == null)
                _setValueAccessorSingle = SetObservableMemberEventValue;
            else
                _setValueAccessorSingle = SetEventValue;
            _canRead = true;
            _canWrite = true;
            _isSingleParameter = true;
        }

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
                if (path.StartsWith("[", StringComparison.Ordinal) || path.StartsWith("Item[", StringComparison.Ordinal))
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
                _canObserve = true;
            }
            _canRead = true;
            _canWrite = true;
        }

        public BindingMemberInfo(IBindingMemberInfo attachedIndexerMember, string path)
        {
            var accessor = new AttachedIndexerAccessor(attachedIndexerMember, BindingReflectionExtensions.GetIndexerValues(path, castType: typeof(string)));
            _getValueAccessor = accessor.GetValue;
            _setValueAccessor = accessor.SetValue;
            _indexerAttachedBindingMember = attachedIndexerMember;
            _memberType = attachedIndexerMember.MemberType;
            _type = attachedIndexerMember.Type;
            _canRead = attachedIndexerMember.CanRead;
            _canWrite = attachedIndexerMember.CanWrite;
            _canObserve = attachedIndexerMember.CanObserve;
        }

        #endregion

        #region Implementation of IBindingMemberInfo

        public string Path => _path;

        public Type Type => _type;

        public object Member => (object)_member ?? _observableMember ?? _indexerAttachedBindingMember;

        public BindingMemberType MemberType => _memberType;

        public bool CanRead => _canRead;

        public bool CanWrite => _canWrite;

        public bool CanObserve => _canObserve;

        public object GetValue(object source, object[] args)
        {
            if (_isSingleParameter)
                return _getValueAccessorSingle(source);
            return _getValueAccessor(source, args);
        }

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

        public object SetSingleValue(object source, object value)
        {
            if (_isSingleParameter)
            {
                if (_setValueAccessorSingleAction == null)
                    return _setValueAccessorSingle(source, value);
                _setValueAccessorSingleAction(source, value);
                return null;
            }
            return _setValueAccessor(source, new[] { value });
        }

        public IDisposable TryObserve(object source, IEventListener listener)
        {
            if (_isDataContext)
                return WeakEventManager.AddBindingContextListener(BindingServiceProvider.ContextManager.GetBindingContext(source), listener, true);
            if (_memberEvent == null)
            {
                if (_isDynamic)
                    return ((IDynamicObject)source).TryObserve(_path, listener);
                return _indexerAttachedBindingMember?.TryObserve(source, listener);
            }
            return _memberEvent.SetSingleValue(source, listener) as IDisposable;
        }

        #endregion

        #region Methods

        private void InitiliazePropertySetter(object o, object o1)
        {
            var p = (PropertyInfo)_member;
            _setValueAccessorSingleAction = p.GetSetPropertyAccessor(p.GetSetMethod(true), _path);
            _setValueAccessorSingleAction(o, o1);
        }

        private object InitiliazePropertyGetter(object o)
        {
            var p = (PropertyInfo)_member;
            _getValueAccessorSingle = p.GetGetPropertyAccessor(p.GetGetMethod(true), _path);
            return _getValueAccessorSingle(o);
        }

        private void InitializeFieldSetter(object o, object o1)
        {
            _setValueAccessorSingleAction = ServiceProvider.ReflectionManager.GetMemberSetter<object>(_member);
            _setValueAccessorSingleAction(o, o1);
        }

        private object InitiliazeFieldGetter(object o)
        {
            _getValueAccessorSingle = ServiceProvider.ReflectionManager.GetMemberGetter<object>(_member);
            return _getValueAccessorSingle(o);
        }

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
            return new BindingActionValue(o, this);
        }

        private object SetObservableMemberEventValue(object o, object arg)
        {
            var listener = arg as IEventListener;
            if (listener == null)
                throw BindingExceptionManager.BindingMemberMustBeWriteable(this);
            return _observableMember.TryObserve(o, listener);
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

        public override string ToString()
        {
            return $"{MemberType}, {Member}";
        }

        #endregion
    }
}