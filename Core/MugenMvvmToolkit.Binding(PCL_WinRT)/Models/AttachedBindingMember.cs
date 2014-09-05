#region Copyright
// ****************************************************************************
// <copyright file="AttachedBindingMember.cs">
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
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the helper class to create attached binding members.
    /// </summary>
    public static class AttachedBindingMember
    {
        #region Nested types

        private interface IAttachedBindingMemberInternal : IBindingMemberInfo
        {
            string MemberChangeEventName { get; set; }

            string Id { get; }

            void UpdateType(Type type);
        }

        private class AttachedBindingMemberInfo<TTarget, TType> : IAttachedBindingMemberInternal, IAttachedBindingMemberInfo<TTarget, TType>
        {
            #region Fields

            private const string IsAttachedHandlerInvokedMember = ".IsAttachedHandlerInvoked";
            private static readonly Func<TTarget, object, AttachedProperty<TTarget, TType>> AttachedPropertyFactoryDelegate;

            private readonly Func<IBindingMemberInfo, TTarget, TType> _getValueSimple;
            private readonly Func<IBindingMemberInfo, TTarget, object[], TType> _getValue;

            private readonly Func<IBindingMemberInfo, TTarget, object[], object> _setValue;
            private readonly Action<IBindingMemberInfo, TTarget, TType> _setValueSimple;

            private readonly Action<TTarget, MemberAttachedEventArgs> _memberAttachedHandler;
            private readonly Action<TTarget, AttachedMemberChangedEventArgs<TType>> _memberChangedHandler;
            private readonly Func<TTarget, IBindingMemberInfo, TType> _defaultValue;
            private readonly Func<IBindingMemberInfo, TTarget, IEventListener, IDisposable> _observeMemberDelegate;

            private static readonly Func<TType, object> GetValueConverter;
            private readonly string _path;
            private readonly BindingMemberType _memberType;
            private readonly MemberInfo _member;
            private readonly bool _canRead;
            private readonly bool _canWrite;
            private readonly string _id;
            private readonly bool _isAttachedProperty;

            private Type _type;

            #endregion

            #region Constructors

            static AttachedBindingMemberInfo()
            {
                AttachedPropertyFactoryDelegate = AttachedPropertyFactory;
                //NOTE Hack to avoid boxing for boolean types.
                if (typeof(bool) == typeof(TType))
                    GetValueConverter = (Func<TType, object>)(object)BooleanToObjectConverter;
                if (typeof(bool?) == typeof(TType))
                    GetValueConverter = (Func<TType, object>)(object)NullableBooleanToObjectConverter;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="AttachedBindingMemberInfo{TTarget,TType}" /> class.
            /// </summary>
            public AttachedBindingMemberInfo(string path, Type type,
                Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler,
                Action<TTarget, AttachedMemberChangedEventArgs<TType>> memberChangedHandler, Func<TTarget, IBindingMemberInfo, TType> defaultValue, BindingMemberType memberType = null)
                : this(path, type, memberAttachedHandler, ObserveAttached, null, GetAttachedValue, null, SetAttachedValue, null, memberType)
            {
                _memberChangedHandler = memberChangedHandler;
                _defaultValue = defaultValue;
                _isAttachedProperty = true;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="AttachedBindingMemberInfo{TTarget,TType}" /> class.
            /// </summary>
            public AttachedBindingMemberInfo(string path, Type type,
                Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler,
                Func<IBindingMemberInfo, TTarget, IEventListener, IDisposable> observeMemberDelegate,
                Func<IBindingMemberInfo, TTarget, object[], TType> getValue, Func<IBindingMemberInfo, TTarget, TType> getValueSimple,
                Func<IBindingMemberInfo, TTarget, object[], object> setValue, Action<IBindingMemberInfo, TTarget, TType> setValueSimple,
                MemberInfo member, BindingMemberType memberType = null)
            {
                Should.NotBeNullOrWhitespace(path, "path");
                Should.NotBeNull(type, "type");
                if (getValue == null)
                    _getValueSimple = getValueSimple ?? GetValueThrow<TTarget, TType>;
                else
                    _getValue = getValue;
                if (setValue == null)
                    _setValueSimple = setValueSimple ?? SetValueThrow;
                else
                    _setValue = setValue;
                _path = path;
                _memberAttachedHandler = memberAttachedHandler;
                _observeMemberDelegate = observeMemberDelegate;
                _type = type ?? typeof(object);
                _member = member;
                _memberType = memberType ?? BindingMemberType.Attached;
                _canRead = getValue != null || getValueSimple != null;
                _canWrite = setValue != null || setValueSimple != null;
                _id = MemberPrefix + Interlocked.Increment(ref _counter).ToString() + "." + path;
                Tracer.Info("The attached property with path: {0}, type: {1}, target type: {2} was created.", path, type, typeof(TTarget));
            }

            #endregion

            #region Implementation of interfaces

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
            ///     Gets the underlying member, if any.
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
            object IBindingMemberInfo.GetValue(object source, object[] args)
            {
                if (GetValueConverter == null)
                    return GetValue((TTarget)source, args);
                return GetValueConverter(GetValue((TTarget)source, args));
            }

            /// <summary>
            ///     Sets the member value of a specified object.
            /// </summary>
            /// <param name="source">The object whose member value will be set.</param>
            /// <param name="args">Optional values for members.</param>
            object IBindingMemberInfo.SetValue(object source, object[] args)
            {
                return SetValue((TTarget)source, args);
            }

            /// <summary>
            ///     Attempts to track the value change.
            /// </summary>
            IDisposable IBindingMemberInfo.TryObserve(object source, IEventListener listener)
            {
                return TryObserve((TTarget)source, listener);
            }

            /// <summary>
            ///     Returns the member value of a specified object.
            /// </summary>
            /// <param name="source">The object whose member value will be returned.</param>
            /// <param name="args">Optional values for members.</param>
            /// <returns>The member value of the specified object.</returns>
            public TType GetValue(TTarget source, object[] args)
            {
                if (_isAttachedProperty && _memberAttachedHandler == null && _defaultValue == null &&
                    !ServiceProvider.AttachedValueProvider.Contains(source, Id))
                    return default(TType);

                if (_memberAttachedHandler != null)
                    RaiseAttached(source);
                if (_getValueSimple == null)
                    return _getValue(this, source, args);
                return _getValueSimple(this, source);
            }

            /// <summary>
            ///     Sets the member value of a specified object.
            /// </summary>
            /// <param name="source">The object whose member value will be set.</param>
            /// <param name="value">The value for member.</param>
            public object SetValue(TTarget source, TType value)
            {
                if (_setValueSimple == null)
                    return SetValue(source, new object[] { value });

                if (_memberAttachedHandler != null)
                    RaiseAttached(source);
                _setValueSimple(this, source, value);
                return null;
            }

            /// <summary>
            ///     Sets the member value of a specified object.
            /// </summary>
            /// <param name="source">The object whose member value will be set.</param>
            /// <param name="args">Optional values for members..</param>
            public object SetValue(TTarget source, object[] args)
            {
                if (_memberAttachedHandler != null)
                    RaiseAttached(source);
                if (_setValueSimple == null)
                    return _setValue(this, source, args);
                if (_isAttachedProperty)
                    GetAttachedProperty(this, source).SetValue(source, (TType)args[0], args);
                else
                    _setValueSimple(this, source, (TType)args[0]);
                return null;
            }

            /// <summary>
            ///     Attempts to track the value change.
            /// </summary>
            public IDisposable TryObserve(TTarget source, IEventListener listener)
            {
                if (_observeMemberDelegate == null)
                    return null;
                return _observeMemberDelegate(this, source, listener);
            }

            public string Id
            {
                get { return _id; }
            }

            public void UpdateType(Type type)
            {
                Should.NotBeNull(type, "type");
                _type = type;
            }

            public string MemberChangeEventName { get; set; }

            #endregion

            #region Methods

            private static IDisposable ObserveAttached(IBindingMemberInfo member, TTarget source, IEventListener listener)
            {
                return GetAttachedProperty((IAttachedBindingMemberInternal)member, source).AddWithUnsubscriber(listener);
            }

            private static void SetAttachedValue(IBindingMemberInfo member, TTarget source, TType value)
            {
                GetAttachedProperty((IAttachedBindingMemberInternal)member, source)
                    .SetValue(source, value, Empty.Array<object>());
            }

            private static TType GetAttachedValue(IBindingMemberInfo member, TTarget source)
            {
                return GetAttachedProperty((IAttachedBindingMemberInternal)member, source).Value;
            }

            private static AttachedProperty<TTarget, TType> GetAttachedProperty(IAttachedBindingMemberInternal member, TTarget source)
            {
                return ServiceProvider
                    .AttachedValueProvider
                    .GetOrAdd(source, member.Id, AttachedPropertyFactoryDelegate, member);
            }

            private static AttachedProperty<TTarget, TType> AttachedPropertyFactory(TTarget source, object state)
            {
                var member = (AttachedBindingMemberInfo<TTarget, TType>)state;
                var property = new AttachedProperty<TTarget, TType> { Value = member.GetDefaultValue(source) };
                if (member._memberChangedHandler != null)
                    property.Member = member;
                return property;
            }

            internal void MemberChanged(TTarget sender, TType oldValue, TType newValue, object[] args)
            {
                _memberChangedHandler(sender, new AttachedMemberChangedEventArgs<TType>(oldValue, newValue, args, this));
            }

            private void RaiseAttached(TTarget source)
            {
                var id = ServiceProvider
                    .AttachedValueProvider
                    .GetValue<object>(source, Id + IsAttachedHandlerInvokedMember, false);
                if (id != null)
                    return;
                id = new object();
                var attachId = ServiceProvider
                    .AttachedValueProvider
                    .GetOrAdd(source, Id + IsAttachedHandlerInvokedMember, (o, o1) => o1, id);
                if (ReferenceEquals(id, attachId))
                    _memberAttachedHandler(source, new MemberAttachedEventArgs(this));
            }

            private TType GetDefaultValue(TTarget source)
            {
                if (_defaultValue == null)
                    return default(TType);
                return _defaultValue(source, this);
            }

            #endregion
        }

        private sealed class AttachedProperty<TTarget, TType> : EventListenerList
        {
            #region Fields

            public AttachedBindingMemberInfo<TTarget, TType> Member;
            public TType Value;

            #endregion

            #region Methods

            public void SetValue(TTarget source, TType value, object[] args)
            {
                if (Equals(Value, value))
                    return;
                var oldValue = Value;
                Value = value;
                if (Member != null)
                    Member.MemberChanged(source, oldValue, value, args);
                Raise(source, EventArgs.Empty);
            }

            #endregion
        }

        private sealed class ObservableProperty<TTarget, TType>
        {
            #region Fields

            private const string ListenerMember = ".EventInvoker";
            private readonly Func<IBindingMemberInfo, TTarget, TType, bool> _setValue;

            #endregion

            #region Constructors

            public ObservableProperty(Func<IBindingMemberInfo, TTarget, TType, bool> setValue)
            {
                Should.NotBeNull(setValue, "setValue");
                _setValue = setValue;
            }

            #endregion

            #region Methods

            public IDisposable ObserveMember(IBindingMemberInfo arg1, TTarget arg2, IEventListener arg3)
            {
                return ServiceProvider
                    .AttachedValueProvider
                    .GetOrAdd(arg2, GetMemberPath(arg1), EventListenerListFactory, null)
                    .AddWithUnsubscriber(arg3);
            }

            public void SetValue(IBindingMemberInfo arg1, TTarget arg2, TType value)
            {
                if (!_setValue(arg1, arg2, value))
                    return;
                var invoker = ServiceProvider
                    .AttachedValueProvider
                    .GetValue<EventListenerList>(arg2, GetMemberPath(arg1), false);
                if (invoker != null)
                    invoker.Raise(arg2, EventArgs.Empty);
            }

            private static string GetMemberPath(IBindingMemberInfo info)
            {
                return ((IAttachedBindingMemberInternal)info).Id + ListenerMember;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static int _counter;
        private const string MemberPrefix = "#@$Attached";
        private static readonly Func<object, object, EventListenerList> EventListenerListFactory;
        private static readonly Func<bool, object> BooleanToObjectConverter;
        private static readonly Func<bool?, object> NullableBooleanToObjectConverter;

        #endregion

        #region Constructors

        static AttachedBindingMember()
        {
            EventListenerListFactory = EventListenerListFactoryMethod;
            BooleanToObjectConverter = Empty.BooleanToObject;
            NullableBooleanToObjectConverter = NullableBooleanToObject;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an attached event member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<TTarget, object> CreateEvent<TTarget>([NotNull] string path,
            Func<IBindingMemberInfo, TTarget, IEventListener, IDisposable> setValue, Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler = null)
        {
            return new AttachedBindingMemberInfo<TTarget, object>(path, typeof(Delegate), memberAttachedHandler, null,
                null, GetBindingMemberValue, (info, o, arg3) => setValue(info, o, (IEventListener)arg3[0]), null, null,
                BindingMemberType.Event);
        }

        /// <summary>
        ///     Creates an attached event member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<object, object> CreateEvent([NotNull] string path,
            Func<IBindingMemberInfo, object, IEventListener, IDisposable> setValue, Action<object, MemberAttachedEventArgs> memberAttachedHandler = null)
        {
            return CreateEvent<object>(path, setValue, memberAttachedHandler);
        }

        /// <summary>
        ///     Creates an attached property member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<TTarget, TType> CreateAutoProperty<TTarget, TType>([NotNull] string path,
            Action<TTarget, AttachedMemberChangedEventArgs<TType>> memberChangedHandler = null,
            Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler = null, Func<TTarget, IBindingMemberInfo, TType> getDefaultValue = null)
        {
            return new AttachedBindingMemberInfo<TTarget, TType>(path, typeof(TType), memberAttachedHandler, memberChangedHandler, getDefaultValue);
        }

        /// <summary>
        ///     Creates an attached property member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<object, object> CreateAutoProperty([NotNull] string path, [NotNull] Type type,
            Action<object, AttachedMemberChangedEventArgs<object>> memberChangedHandler = null,
            Action<object, MemberAttachedEventArgs> memberAttachedHandler = null, Func<object, IBindingMemberInfo, object> getDefaultValue = null)
        {
            return CreateAutoProperty(path, memberChangedHandler, memberAttachedHandler, getDefaultValue)
                .UpdateType(type);
        }

        /// <summary>
        ///     Creates an attached member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<TTarget, TType> CreateMember<TTarget, TType>([NotNull] string path,
            [CanBeNull]Func<IBindingMemberInfo, TTarget, TType> getValue, [CanBeNull] Action<IBindingMemberInfo, TTarget, TType> setValue,
            string memberChangeEventName = null, Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null)
        {
            Func<IBindingMemberInfo, TTarget, IEventListener, IDisposable> observeMember = null;
            if (!string.IsNullOrEmpty(memberChangeEventName))
                observeMember = ObserveMemberChangeEvent;
            return CreateMember(path, getValue, setValue, observeMember, memberAttachedHandler, member)
                .SetEventName(memberChangeEventName);
        }

        /// <summary>
        ///     Creates an attached member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<object, object> CreateMember([NotNull] string path, [NotNull] Type type,
            [CanBeNull]Func<IBindingMemberInfo, object, object> getValue, [CanBeNull]Action<IBindingMemberInfo, object, object> setValue,
            string memberChangeEventName = null, Action<object, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null)
        {
            return CreateMember(path, getValue, setValue, memberChangeEventName, memberAttachedHandler, member)
                .UpdateType(type);
        }

        /// <summary>
        ///     Creates an attached member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<TTarget, TType> CreateMember<TTarget, TType>([NotNull] string path,
            [CanBeNull]Func<IBindingMemberInfo, TTarget, TType> getValue,
            [CanBeNull]Action<IBindingMemberInfo, TTarget, TType> setValue,
            [CanBeNull]Func<IBindingMemberInfo, TTarget, IEventListener, IDisposable> observeMemberDelegate,
            Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null)
        {
            return new AttachedBindingMemberInfo<TTarget, TType>(path, typeof(TType), memberAttachedHandler,
                observeMemberDelegate, null, getValue, null, setValue, member);
        }

        /// <summary>
        ///     Creates an attached member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<object, object> CreateMember([NotNull] string path, [NotNull] Type type,
            [CanBeNull]Func<IBindingMemberInfo, object, object> getValue,
            [CanBeNull]Action<IBindingMemberInfo, object, object> setValue,
            [CanBeNull]Func<IBindingMemberInfo, object, IEventListener, IDisposable> observeMemberDelegate,
            Action<object, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null)
        {
            return CreateMember(path, getValue, setValue, observeMemberDelegate, memberAttachedHandler, member)
                .UpdateType(type);
        }

        /// <summary>
        ///     Creates an attached notifiable member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<TTarget, TType> CreateNotifiableMember<TTarget, TType>([NotNull] string path,
            [CanBeNull]Func<IBindingMemberInfo, TTarget, TType> getValue,
            [NotNull]Func<IBindingMemberInfo, TTarget, TType, bool> setValue,
            Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null)
        {
            var observableProperty = new ObservableProperty<TTarget, TType>(setValue);
            return new AttachedBindingMemberInfo<TTarget, TType>(path, typeof(TType), memberAttachedHandler,
                observableProperty.ObserveMember, null, getValue, null, observableProperty.SetValue, member);
        }

        /// <summary>
        ///     Creates an attached notifiable member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<object, object> CreateNotifiableMember([NotNull] string path, [NotNull] Type type,
            [CanBeNull]Func<IBindingMemberInfo, object, object> getValue, [NotNull]Func<IBindingMemberInfo, object, object, bool> setValue,
            Action<object, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null)
        {
            return CreateNotifiableMember(path, getValue, setValue, memberAttachedHandler, member)
                .UpdateType(type);
        }

        /// <summary>
        ///     Creates an attached member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<TTarget, TType> CreateMember<TTarget, TType>([NotNull] string path,
            [CanBeNull]Func<IBindingMemberInfo, TTarget, object[], TType> getValue,
            [CanBeNull]Func<IBindingMemberInfo, TTarget, object[], object> setValue = null,
            [CanBeNull]Func<IBindingMemberInfo, TTarget, IEventListener, IDisposable> observeMemberDelegate = null,
            Action<TTarget, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null, BindingMemberType memberType = null)
        {
            return new AttachedBindingMemberInfo<TTarget, TType>(path, typeof(TType), memberAttachedHandler,
                observeMemberDelegate, getValue, null, setValue, null, member, memberType);
        }

        /// <summary>
        ///     Creates an attached member with custom logic.
        /// </summary>
        public static IAttachedBindingMemberInfo<object, object> CreateMember([NotNull] string path, [NotNull] Type type,
            [CanBeNull]Func<IBindingMemberInfo, object, object[], object> getValue,
            [CanBeNull]Func<IBindingMemberInfo, object, object[], object> setValue = null,
            [CanBeNull]Func<IBindingMemberInfo, object, IEventListener, IDisposable> observeMemberDelegate = null,
            Action<object, MemberAttachedEventArgs> memberAttachedHandler = null, MemberInfo member = null, BindingMemberType memberType = null)
        {
            return CreateMember(path, getValue, setValue, observeMemberDelegate, memberAttachedHandler, member, memberType)
                .UpdateType(type);
        }

        private static TType GetValueThrow<TTarget, TType>(IBindingMemberInfo member, TTarget source)
        {
            throw BindingExceptionManager.BindingMemberMustBeReadable(member);
        }

        private static void SetValueThrow<TTarget, TType>(IBindingMemberInfo member, TTarget source, TType value)
        {
            throw BindingExceptionManager.BindingMemberMustBeWriteable(member);
        }

        private static object GetBindingMemberValue<TTarget>(IBindingMemberInfo bindingMemberInfo, TTarget target)
        {
            return new BindingMemberValue(target, bindingMemberInfo);
        }

        private static IDisposable ObserveMemberChangeEvent<TTarget>(IBindingMemberInfo member, TTarget source,
            IEventListener arg3)
        {
            string eventName = ((IAttachedBindingMemberInternal)member).MemberChangeEventName;
            return BindingServiceProvider.WeakEventManager.TrySubscribe(source, eventName, arg3);
        }

        private static EventListenerList EventListenerListFactoryMethod(object item, object state)
        {
            return new EventListenerList();
        }

        private static T UpdateType<T>(this T member, Type type)
            where T : IBindingMemberInfo
        {
            ((IAttachedBindingMemberInternal)member).UpdateType(type);
            return member;
        }

        private static T SetEventName<T>(this T member, string eventName)
            where T : IBindingMemberInfo
        {
            ((IAttachedBindingMemberInternal)member).MemberChangeEventName = eventName;
            return member;
        }

        private static object NullableBooleanToObject(bool? b)
        {
            if (b == null)
                return null;
            return Empty.BooleanToObject(b.Value);
        }

        #endregion
    }
}