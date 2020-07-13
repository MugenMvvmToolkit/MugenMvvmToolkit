using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Build
{
    [StructLayout(LayoutKind.Auto)]
    public struct BindingSet<TSource> : IDisposable
        where TSource : class
    {
        #region Fields

        private readonly TSource? _source;
        private IBindingManager? _bindingManager;
        private TargetSourceDictionary? _builders;

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

        private IBindingManager BindingManager
        {
            get
            {
                if (_bindingManager == null)
                    _bindingManager = _bindingManager.DefaultIfNull();
                return _bindingManager;
            }
        }

        private TargetSourceDictionary Builders
        {
            get
            {
                if (_builders == null)
                    _builders = new TargetSourceDictionary();
                return _builders;
            }
        }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            Build();
        }

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
            var list = ItemOrListEditor.Get<IBinding>();
            BuildInternal(false, ref list, metadata);
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> BuildIncludeBindings(IReadOnlyMetadataContext? metadata = null)
        {
            ItemOrListEditor<IBinding, List<IBinding>> list = ItemOrListEditor.Get<IBinding>();
            BuildInternal(true, ref list, metadata);
            return list.ToItemOrList<IReadOnlyList<IBinding>>();
        }

        private void BuildInternal(bool includeBindings, ref ItemOrListEditor<IBinding, List<IBinding>> bindings, IReadOnlyMetadataContext? metadata = null)
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
                    if (expressions.IsNullOrEmpty())
                        expressions = ItemOrList.FromRawValue<IBindingBuilder, IReadOnlyList<IBindingBuilder>>(builder.Value, true);

                    foreach (var exp in expressions.Iterator())
                    {
                        var binding = exp.Build(builder.Key.Item1, builder.Key.Item2, metadata);
                        if (includeBindings)
                            bindings.Add(binding);
                    }
                }
            }

            Builders.Clear();
        }

        private void AddBuilder(object target, object? source, ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> expressions)
        {
            Should.NotBeNull(target, nameof(target));
            var key = (target, source);
            Builders.TryGetValue(key, out var value);
            var list = ItemOrList.FromRawValue<IBindingBuilder, List<IBindingBuilder>>(value, true).Editor();
            list.AddRange(expressions);
            Builders[key] = list.GetRawValue();
        }

        #endregion

        #region Nested types

        private sealed class TargetSourceDictionary : LightDictionary<(object, object?), object?>
        {
            #region Methods

            protected override bool Equals((object, object?) x, (object, object?) y)
            {
                return ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);
            }

            protected override int GetHashCode((object, object?) key)
            {
                if (key.Item2 == null)
                    return RuntimeHelpers.GetHashCode(key.Item1);
                return HashCode.Combine(RuntimeHelpers.GetHashCode(key.Item1), RuntimeHelpers.GetHashCode(key.Item2));
            }

            #endregion
        }

        #endregion
    }
}