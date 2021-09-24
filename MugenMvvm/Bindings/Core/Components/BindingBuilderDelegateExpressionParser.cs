using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingBuilderDelegateExpressionParser : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent
    {
        private static readonly MethodInfo GetRequestMethod =
            typeof(BindingBuilderDelegateExpressionParser).GetMethodOrThrow(nameof(GetRequest), BindingFlags.Static | BindingFlags.Public);

        private readonly Dictionary<object, object?> _cache;
        private readonly object[] _singleValueArray;

        public BindingBuilderDelegateExpressionParser(int priority = BindingComponentPriority.BuilderCache)
            : base(priority)
        {
            _singleValueArray = new object[1];
            _cache = new Dictionary<object, object?>(47, InternalEqualityComparer.Reference);
        }

        [Preserve(Conditional = true)]
        public static object? GetRequest<T1, T2>(BindingBuilderDelegate<T1, T2> buildDelegate) where T1 : class where T2 : class => buildDelegate(default).GetRawValue();

        public ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is Delegate del)
            {
                if (!_cache.TryGetValue(del, out var value))
                {
                    value = GetExpression(del, bindingManager, metadata);
                    _cache[del] = value;
                }

                if (value != this)
                    return ItemOrIReadOnlyList.FromRawValue<IBindingBuilder>(value);
            }

            return Components.TryParseBindingExpression(bindingManager, expression, metadata);
        }

        protected override void Invalidate(object? state, IReadOnlyMetadataContext? metadata) => _cache.Clear();

        private object? GetExpression(Delegate del, IBindingManager bindingManager, IReadOnlyMetadataContext? metadata)
        {
            var type = del.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BindingBuilderDelegate<,>))
            {
                if (del.HasClosure())
                    ExceptionManager.ThrowCannotUseExpressionClosure(del);
                _singleValueArray[0] = del;
                return Components
                       .TryParseBindingExpression(bindingManager, GetRequestMethod.MakeGenericMethod(type.GetGenericArguments()).Invoke(null, _singleValueArray)!, metadata)
                       .GetRawValue();
            }

            return this;
        }
    }
}