﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core
{
    [StructLayout(LayoutKind.Auto)]
    public struct BindingSet<TSource> : IDisposable
        where TSource : class
    {
        #region Fields

        private readonly TSource? _source;
        private IBindingManager? _bindingManager;
        private Dictionary<(object, object?), object?>? _builders;

        #endregion

        #region Constructors

        public BindingSet(IBindingManager? bindingManager = null) : this(null, bindingManager)
        {
        }

        public BindingSet(TSource? source, IBindingManager? bindingManager = null)
        {
            _source = source;
            _bindingManager = bindingManager.DefaultIfNull();
            _builders = null;
        }

        #endregion

        #region Properties

        private IBindingManager BindingManager => _bindingManager ??= _bindingManager.DefaultIfNull();

        private Dictionary<(object, object?), object?> Builders => _builders ??= new Dictionary<(object, object?), object?>(InternalEqualityComparer.ValueTupleReference);

        #endregion

        #region Implementation of interfaces

        public void Dispose() => Build();

        #endregion

        #region Methods

        public BindingSet<TSource> Bind<TTarget>(TTarget target, BindingBuilderDelegate<TTarget, TSource> getBuilder, IReadOnlyMetadataContext? metadata = null)
            where TTarget : class
        {
            var expressions = BindingManager.ParseBindingExpression(getBuilder, metadata);
            AddBuilder(target, _source, expressions);
            return this;
        }

        public BindingSet<TSource> Bind<TTarget, T>(TTarget target, T? source, BindingBuilderDelegate<TTarget, T> getBuilder, IReadOnlyMetadataContext? metadata = null)
            where TTarget : class
            where T : class
        {
            var expressions = BindingManager.ParseBindingExpression(getBuilder, metadata);
            AddBuilder(target, source, expressions);
            return this;
        }

        public BindingSet<TSource> Bind<TTarget>(TTarget target, string expression, object? source = null, IReadOnlyMetadataContext? metadata = null)
            where TTarget : class
        {
            var expressions = BindingManager.ParseBindingExpression(expression, metadata);
            AddBuilder(target, source ?? _source, expressions);
            return this;
        }

        public void Build(IReadOnlyMetadataContext? metadata = null)
        {
            var list = new ItemOrListEditor<IBinding>();
            BuildInternal(false, ref list, metadata);
        }

        public ItemOrIReadOnlyList<IBinding> BuildIncludeBindings(IReadOnlyMetadataContext? metadata = null)
        {
            var list = new ItemOrListEditor<IBinding>();
            BuildInternal(true, ref list, metadata);
            return list.ToItemOrList();
        }

        private void BuildInternal(bool includeBindings, ref ItemOrListEditor<IBinding> bindings, IReadOnlyMetadataContext? metadata = null)
        {
            foreach (var builder in Builders)
            {
                if (builder.Value is IBindingBuilder expression)
                {
                    var binding = expression.Build(builder.Key.Item1, builder.Key.Item2, metadata);
                    if (includeBindings)
                        bindings.Add(binding);
                }
                else
                {
                    //note post handler sorting expressions if need
                    var expressions = BindingManager.TryParseBindingExpression(builder.Value!, metadata);
                    if (expressions.IsEmpty)
                        expressions = ItemOrIReadOnlyList.FromRawValue<IBindingBuilder>(builder.Value);

                    foreach (var exp in expressions)
                    {
                        var binding = exp.Build(builder.Key.Item1, builder.Key.Item2, metadata);
                        if (includeBindings)
                            bindings.Add(binding);
                    }
                }
            }

            Builders.Clear();
        }

        private void AddBuilder(object target, object? source, ItemOrIReadOnlyList<IBindingBuilder> expressions)
        {
            Should.NotBeNull(target, nameof(target));
            var key = (target, source);
            Builders.TryGetValue(key, out var value);
            var list = ItemOrListEditor<IBindingBuilder>.FromRawValue(value);
            list.AddRange(expressions);
            Builders[key] = list.GetRawValue();
        }

        #endregion
    }
}