﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

#pragma warning disable CS8714

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct DecoratorsConfiguration<T> : IDecoratorsConfiguration
    {
        public readonly IReadOnlyObservableCollection Collection;
        public readonly int Priority;
        public readonly int Step;
        public readonly bool AllowNull;

        public DecoratorsConfiguration(IReadOnlyObservableCollection collection, int priority, int step, bool allowNull)
        {
            Should.NotBeNull(collection, nameof(collection));
            Step = step;
            Collection = collection;
            Priority = priority;
            AllowNull = allowNull;
        }

        public DecoratorsConfiguration<object?> Configuration => this!;

        public static implicit operator ObservableSet<T>(DecoratorsConfiguration<T> configuration) => (ObservableSet<T>) configuration.Collection;

        public static implicit operator ObservableList<T>(DecoratorsConfiguration<T> configuration) => (ObservableList<T>) configuration.Collection;

        public static implicit operator DecoratorsConfiguration<T>(DecoratorsConfiguration<object> configuration) => configuration.For<T>();

        public static implicit operator DecoratorsConfiguration<object>(DecoratorsConfiguration<T> configuration) => configuration.For<object>();

        public IReadOnlyObservableCollection<TTo> CastCollectionTo<TTo>() => (IReadOnlyObservableCollection<TTo>) Collection;

        public ObservableList<TTo> CastToList<TTo>() => (ObservableList<TTo>) Collection;

        public ObservableSet<TTo> CastToSet<TTo>() => (ObservableSet<TTo>) Collection;

        public DecoratorsConfiguration<TTo> For<TTo>(bool? allowNull = null) => new(Collection, Priority, Step, allowNull.GetValueOrDefault(AllowNull));

        public DecoratorsConfiguration<T> Add(IComponent<IReadOnlyObservableCollection> decorator, int? priority = null) => Add(decorator, priority, out _);

        public DecoratorsConfiguration<T> Add(IComponent<IReadOnlyObservableCollection> decorator, int? priority, out ActionToken removeToken)
        {
            if (Collection == null)
                ExceptionManager.ThrowObjectNotInitialized(typeof(DecoratorsConfiguration<T>));
            removeToken = Collection.AddComponent(decorator);
            return UpdatePriority(priority);
        }

        public DecoratorsConfiguration<T> UpdatePriority(int? priority = null) => new(Collection, priority ?? Priority - Step, Step, AllowNull);

        public DecoratorsConfiguration<T> WithRemoveToken(IComponent<IReadOnlyObservableCollection> decorator, out ActionToken removeToken)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            removeToken = Collection.GetRemoveComponentToken(decorator);
            return this;
        }

        public DecoratorsConfiguration<TTo> OfType<TTo>() => OfType<TTo>(out _);

        public DecoratorsConfiguration<TTo> OfType<TTo>(out ActionToken removeToken)
        {
            var oldAllowNull = AllowNull;
            var configuration = For<object>(true).Where((t, _) => t is TTo, out var decorator);
            removeToken = Collection.GetRemoveComponentToken(decorator);
            return configuration.For<TTo>(oldAllowNull);
        }

        public DecoratorsConfiguration<T> Subscribe<TState>(Func<TrackerCollectionDecorator<T, TState>, T, TState?, int, TState> onAdded,
            Func<TrackerCollectionDecorator<T, TState>, T, TState, int, TState> onRemoved,
            Func<TrackerCollectionDecorator<T, TState>, T, TState, int, object?, TState>? onChanged = null,
            Action<TrackerCollectionDecorator<T, TState>>? onReset = null, Func<T, bool>? immutableCondition = null,
            IEqualityComparer<T>? comparer = null) => Subscribe(onAdded, onRemoved, onChanged, onReset, immutableCondition, comparer, out _);

        public DecoratorsConfiguration<T> Subscribe<TState>(Func<TrackerCollectionDecorator<T, TState>, T, TState?, int, TState> onAdded,
            Func<TrackerCollectionDecorator<T, TState>, T, TState, int, TState> onRemoved,
            Func<TrackerCollectionDecorator<T, TState>, T, TState, int, object?, TState>? onChanged,
            Action<TrackerCollectionDecorator<T, TState>>? onReset, Func<T, bool>? immutableCondition,
            IEqualityComparer<T>? comparer, out TrackerCollectionDecorator<T, TState> decorator)
        {
            decorator = new TrackerCollectionDecorator<T, TState>(Priority, AllowNull, onAdded, onRemoved, onChanged, onReset, immutableCondition, comparer);
            return Add(decorator);
        }
    }
}