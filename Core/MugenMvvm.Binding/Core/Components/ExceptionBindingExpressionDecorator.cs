using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class ExceptionBindingExpressionDecorator : ComponentDecoratorBase<IBindingManager, IBindingExpressionBuilderComponent>, IBindingExpressionBuilderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>([DisallowNull]in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                var result = Components.TryBuildBindingExpression(expression, metadata);
                if (result.Item != null)
                    return new ExceptionWrapperBindingExpression(result.Item);

                var items = result.List;
                if (items != null)
                {
                    var expressions = new IBindingExpression[items.Count];
                    for (var i = 0; i < expressions.Length; i++)
                        expressions[i] = new ExceptionWrapperBindingExpression(items[i]);
                    return expressions;
                }

                BindingExceptionManager.ThrowCannotParseExpression(expression);
            }
            catch (Exception e)
            {
                return new InvalidBinding(e);
            }

            return default;
        }

        #endregion

        #region Nested types

        private sealed class ExceptionWrapperBindingExpression : IBindingExpression, IWrapper<IBindingExpression>
        {
            #region Fields

            private readonly IBindingExpression _bindingExpression;

            #endregion

            #region Constructors

            public ExceptionWrapperBindingExpression(IBindingExpression bindingExpression)
            {
                _bindingExpression = bindingExpression;
            }

            #endregion

            #region Properties

            IBindingExpression IWrapper<IBindingExpression>.Target => _bindingExpression;

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                try
                {
                    return _bindingExpression.Build(target, source, metadata);
                }
                catch (Exception e)
                {
                    return new InvalidBinding(e);
                }
            }

            #endregion
        }

        #endregion
    }
}