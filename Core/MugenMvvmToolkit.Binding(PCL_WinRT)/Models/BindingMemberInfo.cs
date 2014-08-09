#region Copyright
// ****************************************************************************
// <copyright file="BindingMemberInfo.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Accessors;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Binding.Models
{
    internal sealed class BindingMemberInfo : IBindingMemberInfo
    {
        #region Fields

        /// <summary>
        ///     Gets the binding context member.
        /// </summary>
        public static readonly BindingMemberInfo BindingContextMember = new BindingMemberInfo(AttachedMemberConstants.DataContext, BindingMemberType.BindingContext);

        /// <summary>
        ///     Gets the empty member.
        /// </summary>
        public static readonly IBindingMemberInfo Empty = new BindingMemberInfo("Empty", BindingMemberType.Empty);

        /// <summary>
        ///     Gets the unset member.
        /// </summary>
        public static readonly IBindingMemberInfo Unset = new BindingMemberInfo("Unset", BindingMemberType.Unset);

        internal static readonly IBindingMemberInfo MultiBindingSourceAccessorMember = new BindingMemberInfo();

        private readonly bool _canRead;
        private readonly bool _canWrite;
        private readonly Func<object, object[], object> _getValueAccessor;
        private readonly Func<object, object> _getValueAccessorSingle;
        private readonly Func<object, object, object> _setValueAccessorSingle;
        private readonly Action<object, object> _setValueAccessorSingleAction;
        private readonly Func<object, object[], object> _setValueAccessor;
        private readonly bool _isDataContext;
        private readonly MemberInfo _member;
        private readonly BindingMemberType _memberType;
        private readonly string _path;
        private readonly Type _type;
        private readonly bool _isSingleParameter;

        private readonly IBindingMemberInfo _memberEvent;

        #endregion

        #region Constructors

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
            _getValueAccessor = (o, objects) => ((MultiBindingSourceAccessor)o).GetRawValueInternal();
            _setValueAccessor = _getValueAccessor;
            _canRead = true;
            _canWrite = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        private BindingMemberInfo(string path, BindingMemberType memberType)
            : this(path, memberType, typeof(object))
        {
            if (memberType == BindingMemberType.BindingContext)
            {
                _isDataContext = true;
                _getValueAccessorSingle = o => BindingProvider.Instance.ContextManager.GetBindingContext(o).DataContext;
                _setValueAccessorSingle = (o, arg) => BindingProvider.Instance.ContextManager.GetBindingContext(o).DataContext = arg;
                _canRead = true;
                _canWrite = true;
                _isSingleParameter = true;
            }
            else if (memberType == BindingMemberType.Empty)
            {
                _getValueAccessorSingle = o => o;
                _setValueAccessorSingle = NotSupportedSetter;
                _canRead = true;
                _canWrite = false;
                _isSingleParameter = true;
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
            _memberEvent = BindingExtensions.TryFindMemberChangeEvent(BindingProvider.Instance.MemberProvider, sourceType, field.Name);
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

            _memberEvent = BindingExtensions.TryFindMemberChangeEvent(BindingProvider.Instance.MemberProvider, sourceType, property.Name);
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
                .GetIndexerValues(null, path, typeof(int))
                .ToArrayFast(o => (int)o);
            _getValueAccessorSingle = o => ((Array)o).GetValue(indexes);
            _setValueAccessorSingleAction = (o, o1) => ((Array)o).SetValue(o1, indexes);
            _canRead = true;
            _canWrite = true;
            _isSingleParameter = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberInfo" /> class.
        /// </summary>
        public BindingMemberInfo(string path)
            : this(path, BindingMemberType.Dynamic, typeof(object))
        {
            _getValueAccessor = (o, args) => ((IDynamicObject)o).GetMember(Path, args);
            _setValueAccessor = (o, args) =>
            {
                ((IDynamicObject)o).SetMember(Path, args);
                return null;
            };
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
        public IDisposable TryObserveMember(object source, IEventListener listener)
        {
            if (_isDataContext)
            {
                IBindingContext context = BindingProvider.Instance.ContextManager.GetBindingContext(source);
                var handler = listener.ToWeakEventHandler<EventArgs>(false);
                context.DataContextChanged += handler.Handle;
                handler.Unsubscriber = new ActionToken(Unsubscribe, new object[] { context, handler });
                return handler;
            }
            if (_memberEvent == null)
                return null;
            return _memberEvent.SetValue(source, new object[] { listener }) as IDisposable;
        }

        private static void Unsubscribe(object arg)
        {
            var array = (object[])arg;
            ((IBindingContext)array[0]).DataContextChanged -= ((ReflectionExtensions.IWeakEventHandler<EventArgs>)array[1]).Handle;
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
            return BindingProvider.Instance.WeakEventManager.TrySubscribe(o, (EventInfo)_member, listener);
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