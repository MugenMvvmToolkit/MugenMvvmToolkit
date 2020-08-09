using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Build.Components
{
    public sealed class BindingBuilderDelegateExpressionParser : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<object, object?> _cache;
        private readonly object[] _singleValueArray;

        private static readonly MethodInfo GetRequestMethod = typeof(BindingBuilderDelegateExpressionParser).GetMethodOrThrow(nameof(GetRequest), BindingFlags.Static | BindingFlags.Public);

        #endregion

        #region Constructors

        public BindingBuilderDelegateExpressionParser()
        {
            _singleValueArray = new object[1];
            _cache = new Dictionary<object, object?>(47, InternalComparer.Reference);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is Delegate del)
            {
                if (!_cache.TryGetValue(del, out var value))
                {
                    value = GetExpression(del, bindingManager, metadata);
                    _cache[del] = value;
                }

                if (value != this)
                    return ItemOrList.FromRawValueReadonly<IBindingBuilder>(value, true);
            }

            return Components.TryParseBindingExpression(bindingManager, expression, metadata);
        }

        #endregion

        #region Methods

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null) => _cache.Clear();

        private object? GetExpression(Delegate del, IBindingManager bindingManager, IReadOnlyMetadataContext? metadata)
        {
            var type = del.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BindingBuilderDelegate<,>))
            {
                if (del.HasClosure())
                    BindingExceptionManager.ThrowCannotUseExpressionClosure(del);
                _singleValueArray[0] = del;
                return Components.TryParseBindingExpression(bindingManager, GetRequestMethod.MakeGenericMethod(type.GetGenericArguments()).Invoke(null, _singleValueArray)!, metadata).GetRawValue();
            }

            return this;
        }

        [Preserve(Conditional = true)]
        public static BindingExpressionRequest GetRequest<T1, T2>(BindingBuilderDelegate<T1, T2> buildDelegate) where T1 : class where T2 : class => buildDelegate(default);

        #endregion
    }
}