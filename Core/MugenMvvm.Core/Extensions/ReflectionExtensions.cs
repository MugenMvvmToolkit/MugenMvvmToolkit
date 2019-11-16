using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Fields

        private static Action<object, PropertyChangedEventHandler>? _unsubscribePropertyChangedDelegate;
        private static Func<IWeakEventHandler<PropertyChangedEventArgs>, PropertyChangedEventHandler>? _createPropertyChangedHandlerDelegate;

        #endregion

        #region Methods

        public static Delegate CompileEx(this LambdaExpression lambdaExpression, IReadOnlyMetadataContext? metadata = null)
        {
            var compiler = MugenService.Optional<ILambdaExpressionCompiler>();
            if (compiler == null)
                return lambdaExpression.Compile();
            return compiler.Compile(lambdaExpression, metadata);
        }

        public static TDelegate CompileEx<TDelegate>(this Expression<TDelegate> lambdaExpression, IReadOnlyMetadataContext? metadata = null) where TDelegate : Delegate
        {
            var compiler = MugenService.Optional<ILambdaExpressionCompiler>();
            if (compiler == null)
                return lambdaExpression.Compile();
            return compiler.Compile<TDelegate>(lambdaExpression, metadata);
        }

        public static bool IsStatic(this MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            if (member is PropertyInfo propertyInfo)
            {
                var method = propertyInfo.CanRead
                    ? propertyInfo.GetGetMethod(true)
                    : propertyInfo.GetSetMethod(true);
                return method != null && method.IsStatic;
            }

            if (member is EventInfo eventInfo)
            {
                var method = (eventInfo.AddMethod ?? eventInfo.RemoveMethod);
                return method != null && method.IsStatic;
            }

            if (member is MethodBase m)
                return m.IsStatic;
            return member is FieldInfo fieldInfo && fieldInfo.IsStatic;
        }

        public static bool IsAnonymousClass(this Type type)
        {
            return type.IsDefined(typeof(CompilerGeneratedAttribute), false) && type.IsClass;
        }

        public static MethodInfo GetMethodOrThrow(this Type type, string name, BindingFlags flags, Type[]? types = null)
        {
            var method = types == null ? type.GetMethod(name, flags) : type.GetMethod(name, flags, null, types, null);
            Should.BeSupported(method != null, type.Name + "." + name);
            return method!;
        }

        public static IWeakEventHandler<TArg> CreateWeakEventHandler<TTarget, TArg>(TTarget target, Action<TTarget, object, TArg> invokeAction,
            Action<object, IWeakEventHandler<TArg>>? unsubscribeAction = null)
            where TTarget : class
        {
            return new WeakEventHandler<TTarget, TArg, object>(target, invokeAction, unsubscribeAction);
        }

        public static TResult CreateWeakDelegate<TTarget, TArg, TResult>(TTarget target, Action<TTarget, object, TArg> invokeAction, Action<object, TResult>? unsubscribeAction,
            Func<IWeakEventHandler<TArg>, TResult> createHandler)
            where TTarget : class
            where TResult : class
        {
            Should.NotBeNull(createHandler, nameof(createHandler));
            var weakEventHandler = new WeakEventHandler<TTarget, TArg, TResult>(target, invokeAction, unsubscribeAction);
            var handler = createHandler(weakEventHandler);
            if (unsubscribeAction == null)
                return handler;
            weakEventHandler.HandlerDelegate = handler;
            return handler;
        }

        public static PropertyChangedEventHandler MakeWeakPropertyChangedHandler<TTarget>(TTarget target, Action<TTarget, object, PropertyChangedEventArgs> invokeAction)
            where TTarget : class
        {
            if (_unsubscribePropertyChangedDelegate == null)
                _unsubscribePropertyChangedDelegate = UnsubscribePropertyChanged;
            if (_createPropertyChangedHandlerDelegate == null)
                _createPropertyChangedHandlerDelegate = CreateHandler;
            return CreateWeakDelegate(target, invokeAction, _unsubscribePropertyChangedDelegate, _createPropertyChangedHandlerDelegate);
        }

        public static bool CanCreateDelegate(this Type delegateType, MethodInfo method, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().CanCreateDelegate(delegateType, method);
        }

        public static Delegate? TryCreateDelegate(this Type delegateType, object? target, MethodInfo method, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().TryCreateDelegate(delegateType, target, method);
        }

        public static Func<object?[], object> GetActivator(this ConstructorInfo constructor, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetActivator(constructor);
        }

        public static TDelegate GetMethodInvoker<TDelegate>(this MethodInfo method, IReflectionDelegateProvider? reflectionDelegateProvider = null)
            where TDelegate : Delegate
        {
            return (TDelegate)reflectionDelegateProvider.DefaultIfNull().GetMethodInvoker(typeof(TDelegate), method);
        }

        public static Delegate GetMethodInvoker(this MethodInfo method, Type delegateType, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMethodInvoker(delegateType, method);
        }

        public static Func<object?, object?[], object?> GetMethodInvoker(this MethodInfo method, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMethodInvoker(method);
        }

        public static Func<object?, TType> GetMemberGetter<TType>(this MemberInfo member, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMemberGetter<TType>(member);
        }

        public static Action<object?, TType> GetMemberSetter<TType>(this MemberInfo member, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMemberSetter<TType>(member);
        }

        public static T GetValueEx<T>(this MemberInfo member, object? target, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMemberGetter<T>(member).Invoke(target);
        }

        public static void SetValueEx<T>(this MemberInfo member, object target, T value, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            reflectionDelegateProvider.DefaultIfNull().GetMemberSetter<T>(member).Invoke(target, value);
        }

        public static object InvokeEx(this ConstructorInfo constructor, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return constructor.InvokeEx(reflectionDelegateProvider, Default.EmptyArray<object>());
        }

        public static object InvokeEx(this ConstructorInfo constructor, params object?[] parameters)
        {
            return constructor.InvokeEx(null, parameters);
        }

        public static object InvokeEx(this ConstructorInfo constructor, IReflectionDelegateProvider? reflectionDelegateProvider = null, params object?[] parameters)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetActivator(constructor).Invoke(parameters);
        }

        public static object? InvokeEx(this MethodInfo method, object? target, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return method.InvokeEx(target, reflectionDelegateProvider, Default.EmptyArray<object>());
        }

        public static object? InvokeEx(this MethodInfo method, object? target, params object?[] parameters)
        {
            return method.InvokeEx(target, null, parameters);
        }

        public static object? InvokeEx(this MethodInfo method, object? target, IReflectionDelegateProvider? reflectionDelegateProvider = null, params object?[] parameters)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMethodInvoker(method).Invoke(target, parameters);
        }

        public static Expression ConvertIfNeed(this Expression? expression, Type type, bool exactly)
        {
            if (expression == null)
                return null!;
            if (type == typeof(void) || type == expression.Type)
                return expression;
            if (!exactly && !expression.Type.IsValueType && !type.IsValueType && type.IsAssignableFrom(expression.Type))
                return expression;
            return Expression.Convert(expression, type);
        }

        internal static void SetValue<TValue>(this PropertyInfo property, object target, TValue value)
        {
            property.SetValue(target, value, Default.EmptyArray<object>());
        }

        internal static void SetValue<TValue>(this FieldInfo field, object target, TValue value)
        {
            field.SetValue(target, value);
        }

        private static void UnsubscribePropertyChanged(object sender, PropertyChangedEventHandler handler)
        {
            if (sender is INotifyPropertyChanged notifyPropertyChanged)
                notifyPropertyChanged.PropertyChanged -= handler;
        }

        private static PropertyChangedEventHandler CreateHandler(IWeakEventHandler<PropertyChangedEventArgs> weakEventHandler)
        {
            return weakEventHandler.Handle;
        }

        #endregion

        #region Nested types

        public interface IWeakEventHandler<in TArg>
        {
            void Handle(object sender, TArg arg);
        }

        private sealed class WeakEventHandler<TTarget, TArg, TDelegate> : IWeakEventHandler<TArg>
            where TTarget : class
            where TDelegate : class
        {
            #region Fields

            private readonly Action<TTarget, object, TArg> _invokeAction;
            private readonly IWeakReference _targetReference;
            private readonly Delegate? _unsubscribeAction;

            public TDelegate? HandlerDelegate;

            #endregion

            #region Constructors

            public WeakEventHandler(TTarget target, Action<TTarget, object, TArg> invokeAction, Delegate? unsubscribeAction)
            {
                Should.NotBeNull(target, nameof(target));
                Should.NotBeNull(invokeAction, nameof(invokeAction));
                _invokeAction = invokeAction;
                _unsubscribeAction = unsubscribeAction;
                _targetReference = target.ToWeakReference();
            }

            #endregion

            #region Implementation of interfaces

            public void Handle(object sender, TArg arg)
            {
                var target = (TTarget?)_targetReference.Target;
                if (target == null)
                {
                    if (_unsubscribeAction != null)
                    {
                        var action = _unsubscribeAction as Action<object, TDelegate>;
                        if (action == null)
                            ((Action<object, IWeakEventHandler<TArg>>)_unsubscribeAction).Invoke(sender, this);
                        else
                            action.Invoke(sender, HandlerDelegate!);
                    }
                }
                else
                    _invokeAction(target, sender, arg);
            }

            #endregion
        }

        #endregion
    }
}