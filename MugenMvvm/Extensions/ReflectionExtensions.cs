using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Fields

        private static Action<object, PropertyChangedEventHandler>? _unsubscribePropertyChangedDelegate;
        private static Func<IWeakEventHandler<PropertyChangedEventArgs>, PropertyChangedEventHandler>? _createPropertyChangedHandlerDelegate;
        private static readonly TypeLightDictionary<bool> HasClosureDictionary = new TypeLightDictionary<bool>(47);

        #endregion

        #region Properties

        public static Func<Delegate, bool> ClosureDetector { get; set; } = DefaultClosureDetector;

        #endregion

        #region Methods

        public static bool HasClosure(this Delegate d)
        {
            if (d.Target == null)
                return false;
            return ClosureDetector(d);
        }

        public static Delegate CompileEx(this LambdaExpression lambdaExpression)
        {
            var compiler = MugenService.Optional<ILambdaExpressionCompiler>();
            if (compiler == null)
                return lambdaExpression.Compile();
            return compiler.Compile(lambdaExpression);
        }

        public static TDelegate CompileEx<TDelegate>(this Expression<TDelegate> lambdaExpression) where TDelegate : Delegate
        {
            var compiler = MugenService.Optional<ILambdaExpressionCompiler>();
            if (compiler == null)
                return lambdaExpression.Compile();
            return compiler.Compile(lambdaExpression);
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

            if (member is Type type)
                return type.IsAbstract && type.IsSealed;

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
            return method;
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

        public static Func<object?[], object> GetActivator(this IReflectionDelegateProvider reflectionDelegateProvider, ConstructorInfo constructor)
        {
            Should.NotBeNull(reflectionDelegateProvider, nameof(reflectionDelegateProvider));
            var result = reflectionDelegateProvider.TryGetActivator(constructor);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IActivatorReflectionDelegateProviderComponent>(reflectionDelegateProvider);
            return result;
        }

        public static Delegate GetActivator(this IReflectionDelegateProvider reflectionDelegateProvider, ConstructorInfo constructor, Type delegateType)
        {
            Should.NotBeNull(reflectionDelegateProvider, nameof(reflectionDelegateProvider));
            var result = reflectionDelegateProvider.TryGetActivator(constructor, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IActivatorReflectionDelegateProviderComponent>(reflectionDelegateProvider);
            return result;
        }

        public static Func<object?, object?[], object?> GetMethodInvoker(this IReflectionDelegateProvider reflectionDelegateProvider, MethodInfo method)
        {
            Should.NotBeNull(reflectionDelegateProvider, nameof(reflectionDelegateProvider));
            var result = reflectionDelegateProvider.TryGetMethodInvoker(method);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMethodReflectionDelegateProviderComponent>(reflectionDelegateProvider);
            return result;
        }

        public static Delegate GetMethodInvoker(this IReflectionDelegateProvider reflectionDelegateProvider, MethodInfo method, Type delegateType)
        {
            Should.NotBeNull(reflectionDelegateProvider, nameof(reflectionDelegateProvider));
            var result = reflectionDelegateProvider.TryGetMethodInvoker(method, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMethodReflectionDelegateProviderComponent>(reflectionDelegateProvider);
            return result;
        }

        public static Delegate GetMemberGetter(this IReflectionDelegateProvider reflectionDelegateProvider, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(reflectionDelegateProvider, nameof(reflectionDelegateProvider));
            var result = reflectionDelegateProvider.TryGetMemberGetter(member, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMemberReflectionDelegateProviderComponent>(reflectionDelegateProvider);
            return result;
        }

        public static Delegate GetMemberSetter(this IReflectionDelegateProvider reflectionDelegateProvider, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(reflectionDelegateProvider, nameof(reflectionDelegateProvider));
            var result = reflectionDelegateProvider.TryGetMemberSetter(member, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMemberReflectionDelegateProviderComponent>(reflectionDelegateProvider);
            return result;
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

        public static TDelegate GetActivator<TDelegate>(this ConstructorInfo constructor, IReflectionDelegateProvider? reflectionDelegateProvider = null)
            where TDelegate : Delegate
        {
            return (TDelegate)reflectionDelegateProvider.DefaultIfNull().GetActivator(constructor, typeof(TDelegate));
        }

        public static Delegate GetActivator(this ConstructorInfo constructor, Type delegateType, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetActivator(constructor, delegateType);
        }

        public static TDelegate GetMethodInvoker<TDelegate>(this MethodInfo method, IReflectionDelegateProvider? reflectionDelegateProvider = null)
            where TDelegate : Delegate
        {
            return (TDelegate)reflectionDelegateProvider.DefaultIfNull().GetMethodInvoker(method, typeof(TDelegate));
        }

        public static Delegate GetMethodInvoker(this MethodInfo method, Type delegateType, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMethodInvoker(method, delegateType);
        }

        public static Func<object?, object?[], object?> GetMethodInvoker(this MethodInfo method, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return reflectionDelegateProvider.DefaultIfNull().GetMethodInvoker(method);
        }

        public static TDelegate GetMemberGetter<TDelegate>(this MemberInfo member, IReflectionDelegateProvider? reflectionDelegateProvider = null) where TDelegate : Delegate
        {
            return (TDelegate)reflectionDelegateProvider.DefaultIfNull().GetMemberGetter(member, typeof(TDelegate));
        }

        public static TDelegate GetMemberSetter<TDelegate>(this MemberInfo member, IReflectionDelegateProvider? reflectionDelegateProvider = null) where TDelegate : Delegate
        {
            return (TDelegate)reflectionDelegateProvider.DefaultIfNull().GetMemberSetter(member, typeof(TDelegate));
        }

        public static Func<TTarget, TType> GetMemberGetter<TTarget, TType>(this MemberInfo member, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return (Func<TTarget, TType>)reflectionDelegateProvider.DefaultIfNull().GetMemberGetter(member, typeof(Func<TTarget, TType>));
        }

        public static Action<TTarget, TType> GetMemberSetter<TTarget, TType>(this MemberInfo member, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return (Action<TTarget, TType>)reflectionDelegateProvider.DefaultIfNull().GetMemberSetter(member, typeof(Action<TTarget, TType>));
        }

        [return: NotNullIfNotNull("expression")]
        public static Expression? ConvertIfNeed(this Expression? expression, Type type, bool exactly)
        {
            if (expression == null)
                return null;
            if (type == typeof(void) || type == expression.Type)
                return expression;
            if (expression.Type == typeof(void))
                return Expression.Block(expression, type == typeof(object) ? NullConstantExpression : (Expression)Expression.Default(type));
            if (!exactly && !expression.Type.IsValueType && !type.IsValueType && type.IsAssignableFrom(expression.Type))
                return expression;
            if (type.IsByRef && expression is ParameterExpression parameterExpression && parameterExpression.IsByRef && parameterExpression.Type == type.GetElementType())
                return expression;
            if (type == typeof(object) && BoxingExtensions.CanBox(expression.Type))
                return Expression.Call(null, BoxingExtensions.GenericBoxMethodInfo.MakeGenericMethod(expression.Type), expression);
            return Expression.Convert(expression, type);
        }

        private static bool DefaultClosureDetector(Delegate d)
        {
            var key = d.Target.GetType();
            lock (HasClosureDictionary)
            {
                if (!HasClosureDictionary.TryGetValue(key, out var value))
                {
                    value = key.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length != 0 ||
                            key.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length != 0;
                    HasClosureDictionary[key] = value;
                }

                return value;
            }
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
                        if (_unsubscribeAction is Action<object, TDelegate> action)
                            action.Invoke(sender, HandlerDelegate!);
                        else
                            ((Action<object, IWeakEventHandler<TArg>>)_unsubscribeAction).Invoke(sender, this);
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