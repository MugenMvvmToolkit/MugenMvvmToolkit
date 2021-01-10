using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingExpressionExceptionDecorator : ComponentDecoratorBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent
    {
        #region Constructors

        public BindingExpressionExceptionDecorator(int priority = BindingComponentPriority.BuilderExceptionDecorator) : base(priority)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                var result = Components.TryParseBindingExpression(bindingManager, expression, metadata);
                if (result.Item != null)
                    return ExceptionWrapperBindingBuilder.Wrap(result.Item);

                var count = result.Count;
                if (count == 0)
                    ExceptionManager.ThrowCannotParseExpression(expression);

                var expressions = new IBindingBuilder[count];
                int index = 0;
                foreach (var item in result)
                    expressions[index++] = ExceptionWrapperBindingBuilder.Wrap(item);
                return expressions;
            }
            catch (Exception e)
            {
                return new InvalidBinding(e);
            }
        }

        #endregion

        #region Nested types

        private sealed class ExceptionWrapperBindingBuilder : IBindingBuilder, IWrapper<IBindingBuilder>
        {
            #region Fields

            private readonly IBindingBuilder _bindingExpression;

            #endregion

            #region Constructors

            private ExceptionWrapperBindingBuilder(IBindingBuilder bindingExpression)
            {
                _bindingExpression = bindingExpression;
            }

            #endregion

            #region Properties

            IBindingBuilder IHasTarget<IBindingBuilder>.Target => _bindingExpression;

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

            #region Methods

            public static ExceptionWrapperBindingBuilder Wrap(IBindingBuilder expression)
            {
                if (expression is ExceptionWrapperBindingBuilder wrapper)
                    return wrapper;
                return new ExceptionWrapperBindingBuilder(expression);
            }

            #endregion
        }

        #endregion
    }
}