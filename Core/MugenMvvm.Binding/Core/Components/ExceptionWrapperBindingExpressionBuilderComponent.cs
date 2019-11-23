using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class ExceptionWrapperBindingExpressionBuilderComponent : ComponentTrackerBase<IBindingManager, IBindingExpressionBuilderComponent>, IBindingExpressionBuilderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                for (var i = 0; i < Components.Length; i++)
                {
                    var list = Components[i].TryBuildBindingExpression(expression, metadata);
                    if (list.Item != null)
                        return new ExceptionWrapperBindingExpression(list.Item);

                    var items = list.List;
                    if (items == null)
                        continue;

                    var expressions = new IBindingExpression[items.Count];
                    for (var j = 0; j < expressions.Length; j++)
                        expressions[j] = new ExceptionWrapperBindingExpression(items[i]);
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

        private sealed class ExceptionWrapperBindingExpression : IBindingExpression
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

            public bool HasMetadata => _bindingExpression.HasMetadata;

            public IReadOnlyMetadataContext Metadata => _bindingExpression.Metadata;

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