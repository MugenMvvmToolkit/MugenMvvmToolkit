using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Internal
{
    public sealed class MugenServiceProvider : IServiceProvider
    {
        private readonly IViewModelManager? _viewModelManager;
        private ConstantExpression? _viewModelManagerConstant;

        public MugenServiceProvider(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
            Factories = new Dictionary<Type, object?>(17, InternalEqualityComparer.Type);
        }

        public Dictionary<Type, object?> Factories { get; }

        public object? GetService(Type serviceType)
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

            if (value is Func<Type, object?> factory)
                return factory.Invoke(serviceType);
            return value;
        }

        private Func<Type, object>? Generate(Type type)
        {
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
                    else
                        expressions[i] = Expression.Constant(parameterInfo.DefaultValue, parameterInfo.ParameterType);
                }

                expression = Expression.New(constructor, expressions);
            }

            return Expression.Lambda<Func<Type, object>>(expression, MugenExtensions.GetParametersExpression<Type>()).CompileEx();
        }
    }
}