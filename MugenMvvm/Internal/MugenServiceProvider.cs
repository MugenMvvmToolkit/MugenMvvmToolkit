using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Internal
{
    public sealed class MugenServiceProvider : IMugenServiceProvider
    {
        private static readonly ParameterExpression[] Parameters =
        {
            MugenExtensions.GetParameterExpression<Type>(),
            MugenExtensions.GetParameterExpression<IReadOnlyMetadataContext>()
        };

        private readonly IViewModelManager? _viewModelManager;
        private ConstantExpression? _viewModelManagerConstant;

        public MugenServiceProvider(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
            Factories = new Dictionary<Type, object?>(17, InternalEqualityComparer.Type);
        }

        public Dictionary<Type, object?> Factories { get; }

        public void RegisterFactory(Type service, Func<Type, IReadOnlyMetadataContext?, object?> factory)
        {
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(factory, nameof(factory));
            Factories[service] = factory;
        }

        public void RegisterSingleton<T>(T instance) where T : class => RegisterSingleton(typeof(T), instance);

        public void RegisterSingleton(Type service, object instance)
        {
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(instance, nameof(instance));
            Factories[service] = instance;
        }

        public object? GetService(Type serviceType, IReadOnlyMetadataContext? metadata)
        {
            object? value;
            lock (Factories)
            {
                if (!Factories.TryGetValue(serviceType, out value))
                {
                    value = Generate(serviceType);
                    Factories[serviceType] = value;
                }
            }

            if (value is Func<Type, IReadOnlyMetadataContext?, object?> factory)
                return factory.Invoke(serviceType, metadata);
            return value;
        }

        public object? GetService(Type serviceType) => GetService(serviceType, null);

        private Func<Type, IReadOnlyMetadataContext?, object>? Generate(Type type)
        {
            if (type.IsGenericTypeDefinition)
                return null;
            var constructors = type.GetConstructors();
            if (constructors.Length != 1)
                return null;

            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            Expression expression;
            if (parameters.Length == 0)
                expression = Expression.New(constructor);
            else
            {
                var expressions = new Expression[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterInfo = parameters[i];
                    if (!parameterInfo.HasDefaultValue)
                        return null;

                    if (_viewModelManager != null && parameterInfo.ParameterType == typeof(IViewModelManager))
                        expressions[i] = _viewModelManagerConstant ??= Expression.Constant(_viewModelManager, parameterInfo.ParameterType);
                    else if (parameterInfo.ParameterType == typeof(IReadOnlyMetadataContext))
                        expressions[i] = Parameters[1];
                    else
                        expressions[i] = Expression.Constant(parameterInfo.DefaultValue, parameterInfo.ParameterType);
                }

                expression = Expression.New(constructor, expressions);
            }

            return Expression.Lambda<Func<Type, IReadOnlyMetadataContext?, object>>(expression, Parameters).CompileEx();
        }
    }
}