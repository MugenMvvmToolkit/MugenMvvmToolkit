using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Internal
{
    public sealed class MugenServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Func<object>?> _factories;
        private readonly IViewModelManager? _viewModelManager;

        public MugenServiceProvider(IViewModelManager? viewModelManager = null, Dictionary<Type, Func<object>?>? factories = null)
        {
            _viewModelManager = viewModelManager;
            _factories = factories ?? new Dictionary<Type, Func<object>?>(17, InternalEqualityComparer.Type);
        }

        public object? GetService(Type serviceType)
        {
            Func<object>? factory;
            lock (_factories)
            {
                if (!_factories.TryGetValue(serviceType, out factory))
                {
                    factory = Generate(serviceType);
                    _factories[serviceType] = factory;
                }
            }

            return factory?.Invoke();
        }

        private Func<object>? Generate(Type type)
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
                        expressions[i] = Expression.Constant(_viewModelManager, parameterInfo.ParameterType);
                    else
                        expressions[i] = Expression.Constant(parameterInfo.DefaultValue, parameterInfo.ParameterType);
                }

                expression = Expression.New(constructor, expressions);
            }

            return Expression.Lambda<Func<object>>(expression, Array.Empty<ParameterExpression>()).CompileEx();
        }
    }
}